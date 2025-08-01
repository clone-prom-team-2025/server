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

// --- Repositories ---
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IProductMediaRepository, ProductMediaRepository>();

// --- Services ---
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IProductMediaService, ProductMediaService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IProductReviewService, ProductReviewService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IEmailService, EmailService>();


// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
}

// --- HTTP request pipeline ---

app.UseSwagger();
app.UseSwaggerUI();

builder.Services.AddAuthorization();

app.MapControllers();

app.Run();
