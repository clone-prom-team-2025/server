using App.Api.Handlers;
using App.Api.Middleware;
using App.Core.Interfaces;
using App.Core.Models.Auth;
using App.Core.Models.FileStorage;
using App.Core.Validations;
using App.Data;
using App.Data.Repositories;
using App.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using LogEventLevel = Serilog.Events.LogEventLevel;
using RollingInterval = Serilog.RollingInterval;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://sellpoint.pp.ua"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- MongoDB settings ---
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

// --- File storage settings
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<MinIOOptions>(builder.Configuration.GetSection("CloudflareR2"));
builder.Services.Configure<ProductMediaKeys>(builder.Configuration.GetSection("ProductMediaKeys"));

// --- Auth sessions settings ---
builder.Services.Configure<SessionsOptions>(builder.Configuration.GetSection("SessionsSettings"));

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
builder.Services.AddSingleton<IUserBanRepository, UserBanRepository>();
builder.Services.AddSingleton<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddSingleton<IStoreCreateRequestRepository, StoreCreateRequestRepository>();
builder.Services.AddSingleton<ICartRepository, CartRepository>();

// --- Services ---
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IProductMediaService, ProductMediaService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IProductReviewService, ProductReviewService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IAvailableFiltersService, AvailableFiltersService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IStoreService, StoreService>();
builder.Services.AddSingleton<ICartService, CartService>();

builder.Services.AddMemoryCache();

// --- Validation ---
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "App API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- Mapper ----
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication("ReferenceToken")
    .AddScheme<AuthenticationSchemeOptions, ReferenceTokenAuthHandler>("ReferenceToken", null);

builder.Services.AddAuthorization();

var env = builder.Environment;

var loggerConfig = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        env.IsDevelopment() ? LogEventLevel.Verbose : LogEventLevel.Information,
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code
    )
    .WriteTo.File(
        "logs/app.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Verbose, // завжди Verbose у файлі
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10_000_000,
        outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
    );

loggerConfig = loggerConfig
    .MinimumLevel.Verbose()
    .MinimumLevel.Override("Microsoft", env.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning);

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// --- Create app ---
var app = builder.Build();

// Включаємо підтримку forwarded headers, щоб коректно отримувати інформацію про клієнта, IP, схему (http/https)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();

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
    await dbContext.CreateUserIndexesAsync();
    await dbContext.CreateStoreCreateRequestsIndexesAsync();

    var dbSeeder = scope.ServiceProvider.GetRequiredService<MongoDbSeeder>();
    await dbSeeder.SeedUserAsync();
}

// --- HTTP request pipeline ---

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();