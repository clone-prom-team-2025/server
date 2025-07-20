using App.Core.Interfaces;
using App.Core.Mapping;
using App.Data;
using App.Data.Repositories;
using App.Services;

var builder = WebApplication.CreateBuilder(args);

// --- MongoDB settings ---
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));

// --- Infrastructure ---
builder.Services.AddSingleton<MongoDbContext>();

// --- Repositories ---
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();

// --- Services ---
builder.Services.AddSingleton<ICategoryService, CategoryService>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Mapper ----
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// --- Create app ---
var app = builder.Build();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();