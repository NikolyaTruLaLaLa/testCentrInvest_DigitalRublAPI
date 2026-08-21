using Application.Mapping;
using Application.Queries.GetClients;
using Domain.Interfaces;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetClientsQueryHandler).Assembly));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// Регистрация репозиториев (инфраструктурный слой)
//builder.Services.AddScoped<IClientRepository, ClientRepository>();
// ... другие зависимости (DbContext и т.д.)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();