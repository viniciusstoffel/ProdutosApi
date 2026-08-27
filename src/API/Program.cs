/*
 * Camada API - único projeto executável da solução. Referencia
 * Application e Infrastructure. Job dela: expor tudo via HTTP e
 * configurar a Injeção de Dependência (é aqui embaixo que isso acontece).
 */

using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

// versão explícita, senão AutoDetect abriria uma conexão extra só pra descobrir
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

/*
 * Container de DI: cada AddX<Interface, Implementação> ensina "quando
 * alguém pedir essa interface, entrega essa classe". O ProductsController
 * pede IProductService no construtor -> o container resolve a cadeia
 * inteira sozinho (AppDbContext -> ProductRepository -> ProductService
 * -> Controller). Ninguém escreve "new" em lugar nenhum do resto do código.
 *
 * Scoped = uma instância nova por requisição HTTP (existe também
 * Singleton, única pra app inteira, e Transient, nova toda vez que
 * pedem). Precisa ser Scoped porque o DbContext do EF Core não pode
 * ser compartilhado entre requisições simultâneas.
 */
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
