using Application;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WebAPI.Middleware;
using WebAPI.Validators;
using WebAPI.Mapping;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

Console.WriteLine($"=== Current environment: {builder.Environment.EnvironmentName} ===");

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<PlatformWalletRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DigitalRuble API", Version = "v1" });
});

if (builder.Environment.IsEnvironment("Testing"))
{
    Console.WriteLine("Using SQLite InMemory for tests");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("DataSource=:memory:"));
}
else
{
    Console.WriteLine("Using PostgreSQL (via AddInfrastructure)");
    builder.Services.AddInfrastructure(builder.Configuration);
}

builder.Services.AddApplication();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// devploy - menyat
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDirectoryBrowser();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();
app.UseDefaultFiles();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!app.Environment.IsEnvironment("Testing"))
    {
        dbContext.Database.Migrate();
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
}



app.Run();