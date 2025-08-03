using App.Api.Middleware;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.FileStorage;
using App.Data;
using App.Data.Repositories;
using App.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation.AspNetCore;
using FluentValidation;
using App.Core.Validations;

var builder = WebApplication.CreateBuilder(args);

// --- MongoDB settings ---
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

// --- File storage settings
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<CloudflareR2Options>(builder.Configuration.GetSection("CloudflareR2"));
builder.Services.Configure<ProductMediaKeys>(builder.Configuration.GetSection("ProductMediaKeys"));

// --- Jwt service settings ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// --- Infrastructure ---
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<MongoDbSeeder>();

// --- Repositories ---
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IProductMediaRepository, ProductMediaRepository>();
builder.Services.AddSingleton<IAvailableFiltersRepository, AvailableFiltersRepository>();

// --- Services ---
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IProductMediaService, ProductMediaService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IProductReviewService, ProductReviewService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IAvailableFiltersService, AvailableFiltersService>();
builder.Services.AddSingleton<IAuthService, AuthService>();

// --- Validation ---
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "App API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token like: **Bearer {your token}**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- Mapper ----
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing in configuration."))
            )
        };
    });

// --- Create app ---
var app = builder.Build();

// Включаємо підтримку forwarded headers, щоб коректно отримувати інформацію про клієнта, IP, схему (http/https)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

// --- Create MongoDB indexes on startup ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await dbContext.CreateCategoryIndexesAsync();
    await dbContext.CreateProductIndexesAsync();
    await dbContext.CreateProductReviewIndexesAsync();
    await dbContext.CreateStoreReviewIndexesAsync();
    await dbContext.CreateAvailableFiltersIndexesAsync();
    
    var dbSeeder = scope.ServiceProvider.GetRequiredService<MongoDbSeeder>();
    await dbSeeder.SeedUserAsync();
}

// --- HTTP request pipeline ---

app.UseSwagger();
app.UseSwaggerUI();

builder.Services.AddAuthorization();

app.MapControllers();

app.Run();
