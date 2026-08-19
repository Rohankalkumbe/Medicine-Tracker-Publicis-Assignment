using MedicineTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers / API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repositories (JSON-file backed). Singleton so the in-memory lock
// actually guards concurrent access to the underlying file.
builder.Services.AddSingleton<IMedicineService, MedicineService>();
builder.Services.AddSingleton<ISaleService, SaleService>();

// CORS - open policy for local development / assessment purposes.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Serve the SPA (wwwroot/index.html, css, js)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to index.html so the SPA loads on any non-API route
app.MapFallbackToFile("index.html");

app.Run();
