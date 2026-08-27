# Guia completo do projeto InterviewApi

Este documento explica o projeto do absoluto zero: os conceitos, os comandos que criaram cada pasta e arquivo, como o código funciona, e como rodar tudo — tanto pelo terminal quanto pelo Visual Studio 2022. A ideia é que você consiga, sozinho, recriar este projeto do nada só de ler este guia.

---

## Índice

1. [Conceitos fundamentais antes de começar](#1-conceitos-fundamentais-antes-de-começar)
2. [O que é Clean Architecture e por que separar em 4 projetos](#2-o-que-é-clean-architecture-e-por-que-separar-em-4-projetos)
3. [Como a Solution e os projetos foram criados (comando por comando)](#3-como-a-solution-e-os-projetos-foram-criados-comando-por-comando)
4. [Como as referências entre projetos funcionam](#4-como-as-referências-entre-projetos-funcionam)
5. [NuGet: o que é e quais pacotes usamos](#5-nuget-o-que-é-e-quais-pacotes-usamos)
6. [Estrutura de pastas explicada arquivo por arquivo](#6-estrutura-de-pastas-explicada-arquivo-por-arquivo)
7. [O fluxo completo de uma requisição](#7-o-fluxo-completo-de-uma-requisição)
8. [Entity Framework Core: DbContext, Configurations e Migrations](#8-entity-framework-core-dbcontext-configurations-e-migrations)
9. [Injeção de Dependência: onde e como](#9-injeção-de-dependência-onde-e-como)
10. [Como rodar pelo terminal](#10-como-rodar-pelo-terminal)
11. [Como rodar no Visual Studio 2022 (passo a passo)](#11-como-rodar-no-visual-studio-2022-passo-a-passo)
12. [Testando os endpoints](#12-testando-os-endpoints)
13. [Problemas comuns e como resolver](#13-problemas-comuns-e-como-resolver)

---

## 1. Conceitos fundamentais antes de começar

Antes de entrar no projeto em si, alguns termos que vão aparecer o tempo todo:

**Solution (`.sln`)** — não é código, é um arquivo de "índice" que agrupa vários projetos `.csproj` para que o Visual Studio (ou o `dotnet`) saiba que eles fazem parte de um mesmo produto. Uma Solution pode ter 1 projeto ou 50 — aqui ela tem 4.

**Project (`.csproj`)** — é a unidade real de compilação. Cada pasta com um `.csproj` dentro vira uma DLL (biblioteca) ou um `.exe` quando compilada. O `.csproj` é um arquivo XML que lista: para qual versão do .NET compilar, quais pacotes NuGet usar, e quais outros projetos ele referencia.

**NuGet** — é o "gerenciador de pacotes" do mundo .NET (equivalente ao `npm` do JavaScript ou ao `pip` do Python). Em vez de escrever do zero o código que conversa com o MySQL, por exemplo, baixamos um pacote pronto (`Pomelo.EntityFrameworkCore.MySql`) que já faz isso.

**Entity Framework Core (EF Core)** — é um ORM (*Object-Relational Mapper*). Ele permite que você escreva `_context.Products.Add(produto)` em C# e ele mesmo gera o `INSERT INTO Products (...)` em SQL por trás. Você trabalha com classes C#, não com SQL cru.

**Code First** — é a filosofia de trabalho onde você **escreve as classes C# primeiro** (`Product.cs`) e deixa o EF Core gerar a estrutura do banco de dados a partir delas. É o oposto de "Database First", onde o banco já existe e você gera as classes a partir dele.

**Migration** — é um "arquivo de instruções" gerado automaticamente pelo EF Core que descreve como transformar o banco de dados (criar tabela, adicionar coluna, etc.). Cada migration é como um commit do Git, mas para o schema do banco: um histórico de mudanças que pode ser aplicado (`update`) ou desfeito.

**Dependency Injection (DI / Injeção de Dependência)** — em vez de uma classe criar (`new`) as coisas que ela precisa, ela **recebe** essas coisas prontas de fora, geralmente pelo construtor. Isso permite trocar a implementação sem mudar quem usa. O ASP.NET Core já vem com um "contêiner de DI" embutido — não precisamos instalar nada a mais para isso.

**DTO (Data Transfer Object)** — uma classe simples, só com propriedades, usada para transportar dados entre camadas (por exemplo, entre o mundo HTTP e a aplicação) sem expor a entidade de domínio diretamente.

---

## 2. O que é Clean Architecture e por que separar em 4 projetos

A ideia central da Clean Architecture é: **as regras de negócio não devem depender de detalhes técnicos** (banco de dados, framework web, etc.). Detalhes técnicos devem depender das regras de negócio — não o contrário.

Isso é chamado de **Dependency Inversion** (inversão de dependência): a camada de dentro (`Domain`) define uma **interface** (`IProductRepository`), e a camada de fora (`Infrastructure`) **implementa** essa interface. O `Domain` nunca sabe que o `Infrastructure` existe.

Na prática, dividimos em 4 projetos, cada um com uma responsabilidade única:

```
Domain          → o que é um "Produto"? Não depende de nada.
Application     → o que a aplicação FAZ com produtos (casos de uso)?
Infrastructure  → COMO acessamos o banco de dados de verdade?
API             → como expomos isso via HTTP?
```

A direção das setas de dependência (quem pode referenciar quem) é:

```
API  ──────────────▶ Application ──────────▶ Domain
 │                                                ▲
 └────────────────▶ Infrastructure ───────────────┘
```

Note que a seta sempre aponta **para dentro**, em direção ao `Domain`. Nada aponta para fora. Isso significa: se um dia você quiser trocar MySQL por PostgreSQL, ou trocar ASP.NET Core por outro framework web, só precisa mexer em `Infrastructure` ou `API` — o `Domain` e a `Application` não mudam uma linha.

---

## 3. Como a Solution e os projetos foram criados (comando por comando)

Tudo começou com a CLI do .NET (`dotnet`), sem usar o Visual Studio para criar nada — o VS só foi usado depois, para abrir o que já existia. Aqui está exatamente o que foi rodado, na ordem, dentro da pasta `dotNetProject`:

### 3.1. Criar a Solution vazia

```bash
dotnet new sln -n InterviewApi
```

- `dotnet new` = "crie algo a partir de um template"
- `sln` = o template de Solution (gera só o arquivo `InterviewApi.sln`, vazio, sem projetos dentro ainda)
- `-n InterviewApi` = o nome que o arquivo vai ter

Depois desse comando existe só `InterviewApi.sln` na pasta — um arquivo de texto que ainda não referencia nenhum projeto.

### 3.2. Criar os 4 projetos

```bash
dotnet new classlib -n Domain -o src/Domain -f net8.0
dotnet new classlib -n Application -o src/Application -f net8.0
dotnet new classlib -n Infrastructure -o src/Infrastructure -f net8.0
dotnet new webapi -n API -o src/API -f net8.0 --use-controllers
```

- `classlib` = template de "Class Library" — um projeto que vira uma DLL, sem `Main`, sem conseguir rodar sozinho. É o que usamos para `Domain`, `Application` e `Infrastructure`, porque nenhum dos três precisa ser executável — eles só existem para serem referenciados por outro projeto.
- `webapi` = template de Web API do ASP.NET Core — esse sim é executável, sobe um servidor HTTP.
- `--use-controllers` = gera a API no estilo "Controllers" (classes com `[ApiController]`) em vez do estilo "Minimal API" (que é o padrão dos templates mais novos). Escolhemos Controllers porque é o padrão mais didático e mais comum em entrevistas.
- `-o src/Domain` = a pasta de saída (*output*). É isso que cria a estrutura `src/Domain`, `src/Application`, etc.
- `-f net8.0` = a *framework* alvo, isto é, para qual versão do .NET compilar.

Cada um desses comandos criou uma pasta com um `.csproj` dentro (e, no caso do `webapi`, também `Program.cs`, `appsettings.json`, uma pasta `Controllers` com um controller de exemplo, etc., que depois apagamos ou substituímos).

### 3.3. Adicionar os projetos à Solution

Criar os `.csproj` não é suficiente — a Solution ainda não sabe que eles existem. Para "linkar":

```bash
dotnet sln InterviewApi.sln add src/Domain/Domain.csproj src/Application/Application.csproj src/Infrastructure/Infrastructure.csproj src/API/API.csproj
```

Isso edita o `InterviewApi.sln` e adiciona uma referência para cada `.csproj`. É por isso que, ao abrir o `.sln` no Visual Studio, os 4 projetos aparecem no "Solution Explorer".

---

## 4. Como as referências entre projetos funcionam

Ter os 4 projetos na mesma Solution não significa que um pode usar classes do outro — isso é um passo separado, chamado **Project Reference**. Sem essa referência, o `Application` não enxergaria a classe `Product` do `Domain`, por exemplo.

Os comandos usados foram:

```bash
dotnet add src/Application/Application.csproj reference src/Domain/Domain.csproj
dotnet add src/Infrastructure/Infrastructure.csproj reference src/Domain/Domain.csproj
dotnet add src/Infrastructure/Infrastructure.csproj reference src/Application/Application.csproj
dotnet add src/API/API.csproj reference src/Application/Application.csproj
dotnet add src/API/API.csproj reference src/Infrastructure/Infrastructure.csproj
```

Cada linha segue o padrão: `dotnet add <quem_vai_referenciar> reference <quem_está_sendo_referenciado>`.

Isso gera, dentro de cada `.csproj`, um bloco assim (exemplo do `API.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
</ItemGroup>
```

Repare que **o `Domain.csproj` não tem nenhum `<ProjectReference>`** — ele não referencia nada, exatamente como o diagrama da seção 2 mostra. Se você tentasse, por engano, escrever `using Infrastructure.Data;` dentro de um arquivo do `Domain`, o projeto simplesmente **não compilaria**, porque não existe essa referência — essa é a garantia física de que a "regra da Clean Architecture" está sendo seguida, não é só um combinado de boas intenções.

---

## 5. NuGet: o que é e quais pacotes usamos

NuGet é tanto o **nome do gerenciador de pacotes** quanto o **nome do repositório público** (`nuget.org`) onde esses pacotes ficam hospedados. Quando você roda `dotnet add package X`, o `dotnet` baixa o pacote `X` de `nuget.org`, guarda uma cópia local em `C:\Users\<você>\.nuget\packages`, e adiciona uma linha no `.csproj` dizendo "este projeto depende do pacote X, versão Y".

Pacotes instalados neste projeto:

| Pacote | Onde | Por quê |
|---|---|---|
| `Pomelo.EntityFrameworkCore.MySql` | `Infrastructure` | É o "driver" que traduz os comandos do EF Core para SQL que o MySQL entende. O Microsoft não mantém um driver oficial de MySQL para EF Core — a comunidade mantém o Pomelo, que é o padrão de mercado. |
| `Microsoft.EntityFrameworkCore.Design` | `API` | Ferramentas usadas **só em tempo de desenvolvimento** para gerar migrations (`dotnet ef migrations add`). Não é usado em tempo de execução — por isso `PrivateAssets="all"` no `.csproj`, que significa "não propague essa dependência para quem referenciar este projeto". |
| `Swashbuckle.AspNetCore` | `API` | Gera a interface do Swagger (a telinha em `/swagger` onde você testa os endpoints pelo navegador) automaticamente a partir dos seus Controllers. |

Comandos usados para instalar:

```bash
dotnet add src/Infrastructure/Infrastructure.csproj package Pomelo.EntityFrameworkCore.MySql --version 8.0.3
dotnet add src/API/API.csproj package Microsoft.EntityFrameworkCore.Design --version 8.0.13
```

O `Swashbuckle.AspNetCore` já veio junto de graça, porque o próprio template `webapi` do `dotnet new` já o inclui por padrão.

**Por que fixamos a versão exata (`8.0.3`, `8.0.13`) em vez de deixar solto?** Porque isso torna o build **reprodutível**: se você rodar `dotnet restore` daqui a 6 meses, vai baixar exatamente as mesmas versões, e não uma versão nova que talvez tenha mudado algo e quebrado o projeto sem avisar.

Depois de rodar esses comandos, o NuGet também instalou automaticamente as **dependências transitivas** — pacotes que o Pomelo e o EF.Design precisam para funcionar (`Microsoft.EntityFrameworkCore`, `MySqlConnector`, etc.). Você não pediu por eles diretamente, mas eles vêm junto.

Para restaurar tudo isso do zero (por exemplo, depois de clonar o repositório do GitHub em outra máquina), o comando é:

```bash
dotnet restore
```

Ele lê todos os `.csproj` da Solution e baixa tudo que falta.

---

## 6. Estrutura de pastas explicada arquivo por arquivo

```
InterviewApi.sln
├── .gitignore
├── README.md
├── GUIA-COMPLETO.md              (este arquivo)
└── src/
    ├── Domain/
    │   ├── Domain.csproj
    │   ├── Entities/
    │   │   └── Product.cs        → a classe que representa um produto
    │   └── Interfaces/
    │       └── IProductRepository.cs   → o "contrato" de como acessar produtos no banco
    │
    ├── Application/
    │   ├── Application.csproj
    │   ├── DTOs/
    │   │   ├── CreateProductDto.cs     → o formato do JSON que a API recebe no POST
    │   │   └── ProductResponseDto.cs   → o formato do JSON que a API devolve
    │   ├── Interfaces/
    │   │   └── IProductService.cs      → o "contrato" da lógica de aplicação
    │   └── Services/
    │       └── ProductService.cs       → a implementação da lógica de aplicação
    │
    ├── Infrastructure/
    │   ├── Infrastructure.csproj
    │   ├── Data/
    │   │   ├── AppDbContext.cs         → a "ponte" do EF Core com o banco
    │   │   └── Migrations/             → o histórico de mudanças no schema
    │   ├── Configurations/
    │   │   └── ProductConfiguration.cs → como a tabela Products deve ser criada
    │   └── Repositories/
    │       └── ProductRepository.cs    → a implementação real de IProductRepository
    │
    └── API/
        ├── API.csproj
        ├── Program.cs                  → o "ponto de entrada" — onde tudo é configurado
        ├── appsettings.json            → configurações (incluindo a connection string)
        ├── Controllers/
        │   └── ProductsController.cs   → recebe as requisições HTTP
        └── Properties/
            └── launchSettings.json     → em qual porta rodar localmente
```

### Domain/Entities/Product.cs

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Uma classe "burra" — só dados, sem lógica. Repare que `Price` é `decimal`, não `double` ou `float`. Isso é importante: tipos de ponto flutuante (`double`/`float`) armazenam números de forma aproximada internamente, o que pode causar erros de centavos em cálculos monetários (por exemplo, `0.1 + 0.2` em `double` não dá exatamente `0.3`). `decimal` foi feito para dinheiro — é exato.

### Domain/Interfaces/IProductRepository.cs

Uma `interface` em C# é um "contrato": ela lista **o que** deve existir (métodos, com nome e assinatura), mas não **como** funciona. Aqui:

```csharp
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
```

Isso diz: "qualquer classe que implementar `IProductRepository` precisa saber buscar todos os produtos, buscar um por id, adicionar um, e deletar um". Quem implementa de verdade é a `ProductRepository`, lá na `Infrastructure` — mas o `Domain` (e a `Application`) só conhecem esse contrato, nunca a implementação.

`Task<T>` aparece em quase tudo porque as operações de banco de dados são **assíncronas** (`async`) — explicado na seção 9.

### Application/DTOs

```csharp
public class CreateProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Price { get; set; }
}
```

Esse é o formato exato que a API espera receber no corpo (`body`) de um `POST`. As anotações (`[Required]`, `[MaxLength]`, `[Range]`) são **Data Annotations** — o ASP.NET Core valida automaticamente essas regras antes mesmo do seu código rodar. Se o JSON enviado não tiver `name`, ou o `price` for negativo, a API já responde `400 Bad Request` sozinha, sem você escrever nenhum `if`.

Por que não usar a entidade `Product` diretamente como formato de entrada/saída da API? Porque a entidade tem campos como `Id` e `CreatedAt` que **não fazem sentido o cliente enviar** (o `Id` é gerado pelo banco; o `CreatedAt` é definido pelo servidor). Separar em DTOs evita que o cliente HTTP consiga, por exemplo, inventar um `CreatedAt` no passado.

### Application/Services/ProductService.cs

Esse é o "cérebro" da aplicação — implementa `IProductService`, e é quem decide **o que fazer** quando alguém quer criar/listar/buscar/deletar um produto. Ele recebe um `IProductRepository` no construtor (isso é a Injeção de Dependência acontecendo):

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
    // ...
}
```

Repare: `ProductService` **não sabe** se `_repository` é a implementação que usa MySQL, PostgreSQL, ou até uma lista em memória usada num teste. Ele só sabe que tem um objeto que cumpre o contrato `IProductRepository`. Isso é o núcleo prático da Injeção de Dependência.

### Infrastructure/Data/AppDbContext.cs

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

`DbContext` é a classe base do EF Core que representa "uma sessão de conversa com o banco de dados". `DbSet<Product> Products` diz "existe uma tabela (ou equivalente) para a entidade `Product`, e eu posso consultá-la/alterá-la através dessa propriedade". É literalmente essa propriedade que permite escrever `_context.Products.Add(...)` em outro lugar do código.

`OnModelCreating` é chamado uma vez, na inicialização, para "montar" o modelo — aqui, em vez de escrever a configuração da tabela dentro do próprio `DbContext` (o que vira uma bagunça conforme o projeto cresce), usamos `ApplyConfigurationsFromAssembly`, que varre todo o projeto procurando por classes que implementam `IEntityTypeConfiguration<T>` — no nosso caso, só existe a `ProductConfiguration`.

### Infrastructure/Configurations/ProductConfiguration.cs

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.CreatedAt).IsRequired();
    }
}
```

Isso é a "Fluent API" do EF Core — uma forma de configurar, com código C#, exatamente como cada propriedade da classe `Product` deve virar uma coluna no banco. O ponto mais importante aqui é `HasColumnType("decimal(18,2)")`: sem isso, o EF Core (via Pomelo) escolheria uma precisão padrão para a coluna, que pode não ser adequada para dinheiro. `decimal(18,2)` significa "até 18 dígitos no total, sendo 2 depois da vírgula" — o suficiente para qualquer preço realista, sem desperdiçar espaço.

### Infrastructure/Repositories/ProductRepository.cs

A implementação de verdade de `IProductRepository`, usando o `AppDbContext`:

```csharp
public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
{
    return await _context.Products
        .AsNoTracking()
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync(cancellationToken);
}
```

`AsNoTracking()` diz ao EF Core "não fique de olho nessa lista para detectar mudanças depois" — como é uma consulta só de leitura (a lista não vai ser editada e salva de volta), isso economiza memória e processamento. Método padrão para qualquer `GET` que só lê dados.

### API/Controllers/ProductsController.cs

O único lugar do projeto que "fala HTTP":

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
    // ...
}
```

`[Route("api/[controller]")]` — o `[controller]` é substituído automaticamente pelo nome da classe sem o sufixo `Controller`, ou seja, vira `api/Products` (o ASP.NET Core trata isso como case-insensitive, então `api/products` também funciona).

`CreatedAtAction(...)` é a forma correta de responder a um `POST` que cria um recurso: além de devolver `201 Created`, ele também preenche o cabeçalho HTTP `Location` com a URL onde esse novo produto pode ser consultado (`GET /api/products/{id}`) — é uma convenção REST.

Note que o `Controller` **não importa nem `Domain`, nem `Infrastructure`** — só `Application` (via `IProductService` e os DTOs). Ele não sabe que existe MySQL, nem `DbContext`, nem `ProductRepository`. Essa é a regra "o Controller não deve acessar o DbContext ou o Repository diretamente" sendo cumprida na prática.

---

## 7. O fluxo completo de uma requisição

Vamos seguir um `POST /api/products` do início ao fim:

```
1. Cliente HTTP (Postman, navegador, curl)
   envia POST /api/products com { "name": "Teclado", "price": 150.90 }
        │
        ▼
2. ASP.NET Core recebe a requisição, olha as rotas registradas,
   encontra ProductsController.Create, e tenta "encaixar" o JSON
   recebido no parâmetro CreateProductDto dto
        │
        ▼
3. Antes mesmo do seu código rodar, o ASP.NET Core valida as
   Data Annotations do DTO ([Required], [Range]...).
   Se algo estiver errado → já responde 400 Bad Request e para aqui.
        │
        ▼
4. ProductsController.Create chama _productService.CreateAsync(dto)
        │
        ▼
5. ProductService.CreateAsync monta um objeto Product (entidade de
   domínio) a partir do DTO, define CreatedAt = DateTime.UtcNow,
   e chama _repository.AddAsync(product)
        │
        ▼
6. ProductRepository.AddAsync chama _context.Products.AddAsync(product)
   e depois _context.SaveChangesAsync() — é só NESSE MOMENTO que o
   EF Core efetivamente monta e executa o SQL
        │
        ▼
7. AppDbContext, através do driver Pomelo, traduz isso em:
   INSERT INTO Products (Name, Price, CreatedAt) VALUES (...)
        │
        ▼
8. MySQL executa o INSERT, gera o Id automaticamente (AUTO_INCREMENT),
   e devolve confirmação
        │
        ▼
9. O EF Core preenche de volta o campo Id no objeto product em memória
        │
        ▼
10. ProductService converte o Product de volta em um
    ProductResponseDto (agora já com o Id preenchido)
        │
        ▼
11. ProductsController devolve 201 Created com o DTO em JSON
        │
        ▼
12. Cliente HTTP recebe a resposta
```

Cada seta representa uma fronteira entre camadas — e em nenhum momento uma camada "pula" outra. O Controller nunca fala direto com o `AppDbContext`, por exemplo — sempre passa pelo `IProductService`.

---

## 8. Entity Framework Core: DbContext, Configurations e Migrations

### Como uma migration é gerada

Depois de escrever `Product.cs` e `ProductConfiguration.cs`, rodamos:

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API --output-dir Data/Migrations
```

O que cada parte significa:

- `dotnet ef migrations add InitialCreate` — "olhe para o meu `DbContext` atual, compare com a última migration salva (não existe nenhuma ainda, então compara com 'banco vazio'), e gere uma nova migration chamada `InitialCreate` com a diferença".
- `--project src/Infrastructure` — onde está o `DbContext` (é lá que o EF Core vai procurar `AppDbContext`).
- `--startup-project src/API` — qual projeto "executável" usar para ler as configurações (a connection string vem do `appsettings.json` da API).
- `--output-dir Data/Migrations` — em qual pasta salvar os arquivos gerados.

Isso gera **3 arquivos** dentro de `Infrastructure/Data/Migrations`:

1. **`20260827142714_InitialCreate.cs`** — o arquivo principal, com dois métodos: `Up()` (o que fazer para aplicar essa migration — no nosso caso, `CREATE TABLE Products`) e `Down()` (como desfazer — `DROP TABLE Products`). O EF Core sabe rodar `Down()` se você precisar reverter uma migration.
2. **`20260827142714_InitialCreate.Designer.cs`** — um arquivo auxiliar, gerado automaticamente, que guarda um "snapshot" do estado do modelo naquele momento específico. Você nunca edita esse arquivo manualmente.
3. **`AppDbContextModelSnapshot.cs`** — o estado **atual e completo** do modelo inteiro (não só o que mudou nesta migration, mas tudo). É comparando esse arquivo com o seu `DbContext` atual que o EF Core sabe o que mudou da próxima vez que você rodar `migrations add`.

O nome do arquivo começa com um timestamp (`20260827142714`) para garantir que, se várias pessoas de uma equipe criarem migrations ao mesmo tempo, elas fiquem em ordem cronológica sem conflito.

### Como uma migration é aplicada ao banco de verdade

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

Isso conecta no banco (usando a connection string do `appsettings.json`), olha uma tabela de controle chamada `__EFMigrationsHistory` (criada automaticamente na primeira vez) para ver quais migrations já foram aplicadas, e roda o `Up()` de todas as que ainda não foram. Como só existe uma migration (`InitialCreate`), ele roda o `CREATE TABLE Products` e registra na `__EFMigrationsHistory` que essa migration já foi aplicada — assim, rodar `database update` de novo não tenta criar a tabela duas vezes.

### E se eu mudar a entidade `Product` no futuro?

Digamos que você queira adicionar um campo `Description`. O fluxo seria:

1. Adicionar `public string? Description { get; set; }` em `Product.cs`
2. (Opcional) Configurar no `ProductConfiguration.cs` (`.HasMaxLength(500)`, por exemplo)
3. Gerar uma nova migration: `dotnet ef migrations add AddProductDescription --project src/Infrastructure --startup-project src/API --output-dir Data/Migrations`
4. Aplicar: `dotnet ef database update --project src/Infrastructure --startup-project src/API`

O EF Core percebe sozinho que `Description` é novo (comparando com o `AppDbContextModelSnapshot.cs`) e gera um `ALTER TABLE Products ADD COLUMN Description ...` dentro do `Up()` dessa nova migration.

---

## 9. Injeção de Dependência: onde e como

Toda a "cola" que decide **qual implementação concreta** vai preencher cada interface fica em um único lugar: `Program.cs`, na `API`. É lá que o "contêiner de DI" do ASP.NET Core é configurado:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

Cada linha `AddScoped<Interface, Implementação>` diz ao ASP.NET Core: "sempre que alguém pedir um `IProductRepository` no construtor, entregue uma instância de `ProductRepository`". É assim que, quando o `ProductsController` declara `IProductService productService` no construtor, o framework automaticamente:

1. Vê que precisa de um `IProductService` → sabe que é `ProductService`
2. Olha o construtor de `ProductService` → vê que ele precisa de um `IProductRepository`
3. Sabe que é `ProductRepository` → olha o construtor dele → precisa de um `AppDbContext`
4. Cria o `AppDbContext` (usando a connection string configurada)
5. Monta a cadeia inteira e entrega um `ProductService` já pronto, com tudo dentro

Você nunca escreve `new ProductService(...)` em lugar nenhum — o framework monta tudo sozinho. Isso é o que permite, por exemplo, escrever testes automatizados no futuro trocando `ProductRepository` por uma implementação falsa (`InMemoryProductRepository`), sem mudar uma linha do `ProductService` ou do `Controller`.

**Por que `AddScoped` e não `AddSingleton` ou `AddTransient`?** Existem 3 "tempos de vida" possíveis:

- `Singleton` — uma única instância para a vida inteira da aplicação (todo mundo compartilha)
- `Scoped` — uma instância nova por requisição HTTP (é a que usamos)
- `Transient` — uma instância nova toda vez que alguém pede

O `DbContext` do EF Core **precisa** ser `Scoped` (ou `Transient`) — ele não é seguro para ser compartilhado entre requisições simultâneas (`Singleton`). Como `ProductRepository` depende do `AppDbContext`, ele também precisa ser `Scoped` — se fosse `Singleton`, ele ficaria "preso" ao primeiro `AppDbContext` criado, o que quebraria depois de um tempo.

---

## 10. Como rodar pelo terminal

Você já rodou isso ao longo da nossa conversa, mas para referência, o fluxo completo do zero:

```bash
dotnet restore
```
Baixa todos os pacotes NuGet necessários.

```bash
dotnet build
```
Compila tudo, sem rodar. Bom para checar se está tudo certo antes de continuar.

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```
Cria (ou atualiza) as tabelas no MySQL, a partir das migrations.

```bash
dotnet run --project src/API
```
Sobe a API. Ele vai imprimir no terminal algo como `Now listening on: http://localhost:5172`. Deixe essa janela aberta — ela roda em primeiro plano.

Depois, no navegador: `http://localhost:5172/swagger`.

Para parar: `Ctrl+C` na janela do terminal.

---

## 11. Como rodar no Visual Studio 2022 (passo a passo)

### 11.1. Abrir o projeto

1. Abra o Visual Studio 2022.
2. Na tela inicial, clique em **"Abrir um projeto ou uma solução"**.
3. Navegue até `C:\Users\vinic\Documents\programing\dotNetProject` e selecione o arquivo **`InterviewApi.sln`**.
4. Aguarde o Visual Studio carregar — ele mostra os 4 projetos (`API`, `Application`, `Domain`, `Infrastructure`) na janela **Solution Explorer** (geralmente à direita).

### 11.2. Restaurar os pacotes NuGet

Normalmente o Visual Studio faz isso sozinho ao abrir a Solution (você vai ver uma barrinha de progresso "Restoring NuGet packages..." no canto inferior). Se não acontecer automaticamente, ou se der erro de referência faltando:

1. Clique com o botão direito em cima do nome da Solution (`Solution 'InterviewApi'`, no topo do Solution Explorer).
2. Clique em **"Restore NuGet Packages"**.

### 11.3. Definir o projeto de inicialização (Startup Project)

O Visual Studio precisa saber **qual dos 4 projetos rodar** quando você aperta "play" — como `Domain`, `Application` e `Infrastructure` são bibliotecas (não têm como "rodar" sozinhas), precisa ser a `API`.

1. Clique com o botão direito no projeto **`API`** no Solution Explorer.
2. Clique em **"Set as Startup Project"**.

O nome do projeto `API` deve aparecer em **negrito** no Solution Explorer — é assim que você confirma que está configurado corretamente.

*(Isso já deve estar certo, porque só existe um projeto executável na Solution — mas é bom saber onde mexer, caso um dia você adicione mais projetos.)*

### 11.4. Conferir/preencher a connection string

Abra `src/API/appsettings.json` (duplo clique no Solution Explorer) e confirme que a senha do MySQL está preenchida corretamente:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=InterviewApi;Uid=root;Pwd=SUA_SENHA_AQUI;"
```

### 11.5. Rodar as migrations pelo Visual Studio (Package Manager Console)

Você pode continuar usando o terminal normalmente (funciona igual dentro ou fora do VS), mas se quiser fazer tudo pela interface do Visual Studio:

1. Menu superior: **Tools → NuGet Package Manager → Package Manager Console**.
2. Uma janela de console abre na parte de baixo do Visual Studio. Ela tem um campo **"Default project"** — selecione **`Infrastructure`** (é onde fica o `DbContext`).
3. Digite:

```powershell
Update-Database -StartupProject API
```

Esse é o equivalente, dentro do Visual Studio, ao `dotnet ef database update` que rodamos pelo terminal. `-StartupProject API` diz de onde ler a connection string (mesma ideia do `--startup-project` do terminal).

Para gerar uma nova migration pelo VS (se um dia precisar), o comando equivalente é:

```powershell
Add-Migration NomeDaMigration -StartupProject API -OutputDir Data/Migrations
```

### 11.6. Rodar a aplicação

Com o projeto `API` definido como Startup Project, aperte **F5** (ou o botão verde de "play" no topo, que deve mostrar o nome `API` ou `https`/`http`).

Isso vai:
- Compilar a Solution inteira
- Subir o servidor
- Abrir automaticamente o navegador padrão em `https://localhost:XXXX/swagger` (a porta vem do `launchSettings.json`)

Se quiser rodar **sem depurar** (mais rápido, sem poder colocar breakpoints), use **Ctrl+F5** em vez de F5.

Para parar, feche a janela do navegador/terminal que abriu, ou clique no quadrado vermelho de "stop" no Visual Studio.

### 11.7. Usando breakpoints (opcional, mas útil para aprender)

Uma vantagem de rodar pelo Visual Studio (em vez do terminal) é poder colocar **breakpoints** — pontos onde a execução para, e você pode inspecionar variáveis:

1. Abra `src/API/Controllers/ProductsController.cs`.
2. Clique na margem esquerda, ao lado da linha `var product = await _productService.CreateAsync(dto, cancellationToken);` — deve aparecer uma bolinha vermelha.
3. Aperte F5 para rodar em modo debug.
4. Faça um `POST /api/products` pelo Swagger.
5. A execução vai **pausar** exatamente naquela linha, e você pode passar o mouse sobre `dto` para ver o que chegou, usar `F10` para avançar linha por linha, etc.

Isso é uma das formas mais úteis de realmente entender o fluxo da seção 7 acontecendo na prática.

---

## 12. Testando os endpoints

Com a API rodando (por qualquer um dos dois métodos acima), você tem 3 formas de testar:

### Pelo Swagger (mais fácil para iniciante)

Acesse `/swagger` no navegador. Cada endpoint aparece expansível — clique em `POST /api/products`, depois em **"Try it out"**, edite o JSON de exemplo, e clique em **"Execute"**.

### Pelo arquivo `API.http` (dentro do Visual Studio ou VS Code)

O arquivo `src/API/API.http` já tem as 4 requisições prontas. No Visual Studio 2022 (versão 17.6+), basta abrir esse arquivo e clicar no link **"Send Request"** que aparece acima de cada bloco.

### Por linha de comando (curl)

```bash
curl -X POST http://localhost:5172/api/products -H "Content-Type: application/json" -d "{\"name\":\"Teclado\",\"price\":150.90}"
```

---

## 13. Problemas comuns e como resolver

**`Access denied for user 'root'@'localhost'`**
A senha no `appsettings.json` está errada ou desatualizada. Confira se bate com a senha real do seu MySQL.

**`Unknown database 'InterviewApi'`**
O banco ainda não foi criado. No MySQL Workbench (ou `mysql.exe`), rode: `CREATE DATABASE InterviewApi CHARACTER SET utf8mb4;` — a migration cria a **tabela**, não o **banco**.

**Erro ao rodar `Update-Database` ou `dotnet ef database update` dizendo que não encontra o `DbContext`**
Confira se está passando `-StartupProject API` (Visual Studio) ou `--startup-project src/API` (terminal) — sem isso, o EF Core não sabe de onde ler a connection string.

**Porta já em uso (`address already in use`)**
Outra instância da API já está rodando em segundo plano. Feche todas as janelas de terminal/debug anteriores, ou troque a porta em `src/API/Properties/launchSettings.json`.

**O projeto `API` não aparece em negrito no Solution Explorer**
Ele não está definido como Startup Project — volte à seção 11.3.

**Erro de compilação dizendo que uma classe de outro projeto "não existe" (`The type or namespace name 'X' could not be found`)**
Falta uma Project Reference — revise a seção 4 e confira se o `.csproj` do projeto que está usando a classe tem o `<ProjectReference>` correto.
