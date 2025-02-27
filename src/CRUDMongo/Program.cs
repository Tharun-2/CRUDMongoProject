var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var settings = builder.Configuration.GetSection("DatabaseSettings");
    return new MongoDbContext(builder.Configuration.GetConnectionString("MongoDb"), settings["DatabaseName"]);
});
builder.Services.AddScoped<IItemRepository, ItemRepository>(); // Register interface with its implementation
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();
app.Run();
