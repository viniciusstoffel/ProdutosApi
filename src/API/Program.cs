/*
 * Camada API - unico projeto executavel da solucao. Referencia
 * Application e Infrastructure. Job dela: expor tudo via HTTP e
 * configurar a Injecao de Dependencia (e aqui embaixo que isso acontece).
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

// versao explicita, senao AutoDetect abriria uma conexao extra so pra descobrir
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

/*
 * Container de DI: cada AddX<Interface, Implementacao> ensina "quando
 * alguem pedir essa interface, entrega essa classe". O ProductsController
 * pede IProductService no construtor -> o container resolve a cadeia
 * inteira sozinho (AppDbContext -> ProductRepository -> ProductService
 * -> Controller). Ninguem escreve "new" em lugar nenhum do resto do codigo.
 *
 * Scoped = uma instancia nova por requisicao HTTP (existe tambem
 * Singleton, unica pra app inteira, e Transient, nova toda vez que
 * pedem). Precisa ser Scoped porque o DbContext do EF Core nao pode
 * ser compartilhado entre requisicoes simultaneas.
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
