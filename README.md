# ProdutosApi

API simples de cadastro de produtos, feita pra treinar/mostrar os fundamentos de
backend em .NET: Clean Architecture, EF Core, MySQL e injeção de dependência.
Nada além disso - projeto pequeno de propósito, sem features demais.

## stack

- .NET 8 / C# 12
- ASP.NET Core Web API (Controllers, não Minimal API)
- Entity Framework Core 8, Code First
- Pomelo.EntityFrameworkCore.MySql 8.0.3 (driver do MySQL pro EF Core)
- MySQL 8
- Swagger pra testar os endpoints direto no navegador

## arquitetura

4 projetos, separados por camada:

```
ProdutosApi.sln
└── src/
    ├── Domain/            # entidade Product + interface IProductRepository
    │                       # não referencia nada, nem sabe que EF Core existe
    │
    ├── Application/        # DTOs + ProductService (a lógica de "o que fazer")
    │                       # só referencia o Domain
    │
    ├── Infrastructure/     # EF Core, AppDbContext, migrations, o repository de
    │                       # verdade. Referencia Domain e Application
    │
    └── API/                 # Controllers + Program.cs (onde a DI é configurada)
                             # referencia Application e Infrastructure
```

direção das dependências:

```
API ──> Application ──> Domain
 └────> Infrastructure ──┘
```

Domain não depende de nada - nem de Application, nem de Infrastructure, nem de
pacote nenhum de banco/web. A Infrastructure é quem implementa o
IProductRepository que o Domain define. O Controller (na API) só conhece a
Application, nunca fala direto com o banco.

fluxo de uma requisição:

```
HTTP -> ProductsController -> IProductService -> ProductService
     -> IProductRepository -> ProductRepository -> AppDbContext -> MySQL
```

## rodando local

precisa ter instalado:
- .NET SDK 8+
- MySQL 8 rodando local
- dotnet-ef:

```bash
dotnet tool install --global dotnet-ef --version 8.0.13
```

### 1. cria o banco

migration cria a tabela, mas não o banco em si - isso é manual:

```sql
CREATE DATABASE ProdutosApi CHARACTER SET utf8mb4;
```

### 2. configura a senha (via user secrets, não no appsettings.json)

o appsettings.json fica só com um placeholder de propósito - a senha real
nunca deveria ir pro git. usa o user secrets do próprio dotnet:

```bash
dotnet user-secrets init --project src/API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=ProdutosApi;Uid=root;Pwd=SUA_SENHA_AQUI;" --project src/API
```

isso guarda a senha fora da pasta do projeto (no seu perfil de usuário do
Windows), então nunca vai ser commitada sem querer.

se o MySQL local não for 8.0.46, ajusta também a versão no src/API/Program.cs:

```csharp
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
```

### 3. roda a migration

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

pra criar uma migration nova (se mudar alguma entidade):

```bash
dotnet ef migrations add NomeDaMigration --project src/Infrastructure --startup-project src/API --output-dir Data/Migrations
```

### 4. roda a API

```bash
dotnet run --project src/API
```

Swagger fica em `/swagger`.

## endpoints

| método | rota                  | o que faz                | respostas |
|--------|-----------------------|---------------------------|-----------|
| POST   | `/api/products`       | cria um produto           | 201, 400  |
| GET    | `/api/products`       | lista todos os produtos   | 200       |
| GET    | `/api/products/{id}`  | busca um produto por id   | 200, 404  |
| DELETE | `/api/products/{id}`  | remove um produto         | 204, 404  |

## exemplo

```bash
curl -X POST http://localhost:5172/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Teclado","price":150.90}'
```

resposta (201):

```json
{
  "id": 1,
  "name": "Teclado",
  "price": 150.90,
  "createdAt": "2026-08-27T14:32:10.123456Z"
}
```
