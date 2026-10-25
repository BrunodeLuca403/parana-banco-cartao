using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Application.Services.Implementations;
using ParanaBanco.Cartoes.Application.Services.Interface;
using ParanaBanco.Cartoes.Core.Repositories;
using ParanaBanco.Cartoes.Infrastructure.Messaging;
using ParanaBanco.Cartoes.Infrastructure.Persistence;
using ParanaBanco.Cartoes.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CartaoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICartaoRepository, CartaoRepository>();
builder.Services.AddScoped<ICartaoService, CartaoService>();

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddHostedService<PropostaGeradaConsumer>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
