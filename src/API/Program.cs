// ============================================================================
// CAMADA: API (camada de apresentacao / entrada do sistema)
// ============================================================================
// Esse projeto (API.csproj) referencia Application E Infrastructure. E o
// unico projeto executavel da solucao inteira (os outros tres - Domain,
// Application, Infrastructure - sao "class library", nao tem como rodar
// sozinhos, so viram DLL). O trabalho da API e:
//   1. Expor a aplicacao via HTTP (feito pelos Controllers)
//   2. "Ligar os fios" da Injecao de Dependencia (feito bem aqui embaixo)
//
// ESSE ARQUIVO E ONDE A INJECAO DE DEPENDENCIA E CONFIGURADA DE VERDADE.
// Leia os comentarios com calma - e o arquivo mais importante do projeto
// pra entender como todas as camadas se encaixam em tempo de execucao.
// ============================================================================

using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

// "var builder = WebApplication.CreateBuilder(args)" cria um objeto
// "builder" que serve pra configurar tudo ANTES da aplicacao comecar a
// rodar de verdade: servicos, configuracoes, connection string, etc.
// "args" sao os argumentos de linha de comando (tipo dotnet run --algo),
// geralmente vazio no nosso caso.
var builder = WebApplication.CreateBuilder(args);

// builder.Configuration da acesso as configuracoes do appsettings.json
// (e tambem de variaveis de ambiente e do User Secrets, se tiver -
// tudo isso e combinado automaticamente pelo ASP.NET Core).
// GetConnectionString("DefaultConnection") vai direto no bloco
// "ConnectionStrings": { "DefaultConnection": "..." } do appsettings.json
// e devolve o texto da connection string.
//
// O "?? throw new ..." e o operador de COALESCENCIA NULA
// (null-coalescing): "se o valor da esquerda for null, executa o da
// direita". Aqui, se a connection string nao estiver configurada em
// lugar nenhum, a aplicacao PARA DE SUBIR imediatamente com um erro
// claro, em vez de deixar isso quebrar mais tarde, de um jeito confuso,
// so quando alguem tentar de fato usar o banco.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");

// MySqlServerVersion diz ao driver Pomelo exatamente qual versao do
// MySQL estamos usando, pra ele gerar SQL compativel com essa versao
// especifica. Poderia usar ServerVersion.AutoDetect(connectionString)
// pra descobrir a versao sozinho, mas isso abriria uma conexao extra
// com o banco so pra perguntar a versao, toda vez que a aplicacao
// sobe - informar direto aqui e mais rapido e evita essa conexao a mais.
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));

// ==========================================================================
// AQUI COMECA A INJECAO DE DEPENDENCIA DE VERDADE
// ==========================================================================
// "builder.Services" e o CONTAINER DE INJECAO DE DEPENDENCIA (DI
// Container) do ASP.NET Core - ele ja vem prontinho, nao precisamos
// instalar nenhum pacote a mais pra ter isso. Pensa nele como uma
// "receita" gigante: cada linha "AddAlgumaCoisa<X, Y>()" ensina o
// container "quando alguem pedir um X, entrega um Y". Depois, quando
// QUALQUER classe do projeto declara no CONSTRUTOR dela que precisa de
// um X (como IProductRepository, IProductService, AppDbContext), o
// ASP.NET Core olha essa receita e monta/entrega o Y sozinho - SEM que
// ninguem escreva "new Y()" em lugar nenhum do resto do codigo. Isso e
// Injecao de Dependencia (DI) na pratica.
// ==========================================================================

// AddDbContext<AppDbContext> ensina o container: "quando alguem pedir
// um AppDbContext, monte um configurado com UseMySql (usando essa
// connection string e essa versao)". Isso e o que faz o AppDbContext,
// la na Infrastructure, conseguir receber um DbContextOptions<AppDbContext>
// ja prontinho no construtor dele, sem precisar montar essa configuracao
// na mao.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Essas duas linhas sao o coracao da Injecao de Dependencia desse
// projeto inteiro:
//
//   "quando alguem pedir IProductRepository, entrega ProductRepository"
//   "quando alguem pedir IProductService,    entrega ProductService"
//
// E gracas a essas duas linhas que:
//   - o ProductsController (que pede IProductService no construtor)
//     recebe um ProductService de verdade, ja com o IProductRepository
//     dele preenchido dentro
//   - o ProductService (que pede IProductRepository no construtor)
//     recebe um ProductRepository de verdade, ja com o AppDbContext
//     dele preenchido dentro
//
// O container monta essa CADEIA INTEIRA sozinho, resolvendo uma
// dependencia de cada vez, de fora pra dentro (primeiro cria o
// AppDbContext, depois usa ele pra criar o ProductRepository, depois
// usa esse repository pra criar o ProductService). E por isso que a
// API (esse Program.cs) e o UNICO lugar do projeto que precisa
// conhecer as implementacoes CONCRETAS (ProductRepository,
// ProductService) - o resto do codigo (Controller, ProductService)
// so conhece as interfaces, nunca as classes concretas.
//
// "AddScoped" define o TEMPO DE VIDA (lifetime) dessas instancias:
// uma instancia NOVA a cada requisicao HTTP que chega (fica viva do
// inicio ao fim de UMA requisicao, e e descartada logo depois).
// Existem outros dois tempos de vida possiveis no ASP.NET Core:
//   AddSingleton  -> uma unica instancia pra aplicacao inteira,
//                    sempre a mesma, compartilhada entre TODAS as
//                    requisicoes que chegam ao mesmo tempo
//   AddTransient  -> uma instancia NOVA toda vez que alguem pede,
//                    mesmo que seja duas vezes dentro da mesma
//                    requisicao
// Usamos Scoped aqui porque o AppDbContext do EF Core NAO E SEGURO
// pra ser compartilhado entre requisicoes simultaneas (usar
// Singleton quebraria isso, porque varias requisicoes ao mesmo
// tempo bagunçariam o estado interno do mesmo DbContext) - e como
// ProductRepository depende do AppDbContext, ele tambem precisa ser
// Scoped (senao ficaria "preso" pra sempre ao primeiro AppDbContext
// que fosse criado, o que da erro depois de um tempo, quando aquele
// DbContext ja tiver sido descartado).
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Essas tres linhas ligam funcionalidades ja prontas do ASP.NET Core:
// AddControllers            -> habilita o uso de Controllers (classes
//                               tipo ProductsController) pra responder
//                               requisicoes HTTP
// AddEndpointsApiExplorer +
// AddSwaggerGen             -> geram automaticamente a documentacao e
//                               a telinha do Swagger (a interface
//                               visual em /swagger onde da pra testar
//                               os endpoints direto pelo navegador)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// "builder.Build()" pega TODA a configuracao acumulada la em cima e
// finalmente monta o objeto "app" - a aplicacao pronta pra rodar.
// Depois desse ponto, nao da mais pra registrar novos servicos no
// container (a fase de configuracao ja terminou, agora e so execucao).
var app = builder.Build();

// app.Environment.IsDevelopment() confere se a aplicacao esta rodando
// no ambiente de Desenvolvimento (isso e definido pela variavel de
// ambiente ASPNETCORE_ENVIRONMENT). So habilitamos o Swagger nesse
// ambiente especifico - em Producao normalmente nao se deixa a
// telinha de testes exposta pra qualquer um acessar.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// UseHttpsRedirection redireciona automaticamente requisicoes que
// chegam via HTTP (sem criptografia) pra HTTPS (com criptografia),
// quando isso e possivel.
app.UseHttpsRedirection();

// MapControllers "liga" as rotas dos Controllers (tipo os [HttpGet],
// [HttpPost] que estao no ProductsController) pro sistema de
// roteamento do ASP.NET Core - sem essa linha aqui, os Controllers
// ate existiriam compilados, mas nenhuma URL chegaria neles de
// verdade.
app.MapControllers();

// app.Run() efetivamente SOBE o servidor e fica escutando
// requisicoes HTTP ate voce parar com Ctrl+C (ou fechar o processo).
// Essa linha "trava" a execucao aqui - qualquer codigo escrito depois
// dela (se tivesse) so rodaria depois do servidor parar de rodar.
app.Run();
