# InterviewApi

## 1. Objetivo

API REST de exemplo para cadastro de produtos, construída com ASP.NET Core e Entity Framework Core.
O foco é demonstrar, em um projeto pequeno e legível, os fundamentos de:

- Clean Architecture (separação em Domain, Application, Infrastructure e API)
- Inversão de dependência (as camadas internas definem as abstrações, as externas implementam)
- Entity Framework Core com abordagem Code First e migrations
- Injeção de dependência nativa do ASP.NET Core

## 2. Tecnologias

- .NET 8 / C# 12
- ASP.NET Core Web API
- Entity Framework Core 8
- Pomelo.EntityFrameworkCore.MySql 8.0.3
- MySQL 8
- Swagger (Swashbuckle)

## 3. Estrutura da solução

```
InterviewApi.sln
└── src/
    ├── Domain/                         # Entidades e abstrações. Sem dependências externas.
    │   ├── Entities/Product.cs
    │   └── Interfaces/IProductRepository.cs
    │
    ├── Application/                    # Casos de uso, DTOs e regras de aplicação.
    │   ├── DTOs/CreateProductDto.cs
    │   ├── DTOs/ProductResponseDto.cs
    │   ├── Interfaces/IProductService.cs
    │   └── Services/ProductService.cs
    │
    ├── Infrastructure/                 # Acesso a dados: EF Core, MySQL, repositórios.
    │   ├── Data/AppDbContext.cs
    │   ├── Data/Migrations/
    │   ├── Configurations/ProductConfiguration.cs
    │   └── Repositories/ProductRepository.cs
    │
    └── API/                            # Exposição HTTP e composição da aplicação.
        ├── Controllers/ProductsController.cs
        ├── Program.cs
        └── appsettings.json
```

Direção das dependências:

```
API ──> Application ──> Domain
 └────> Infrastructure ──┘
```

O `Domain` não referencia nenhum outro projeto. A `Infrastructure` implementa a interface
`IProductRepository` definida no `Domain`, e é registrada no contêiner de DI pela `API`.

Fluxo de uma requisição:

```
HTTP → ProductsController → IProductService → ProductService
     → IProductRepository → ProductRepository → AppDbContext → MySQL
```

## 4. Pré-requisitos

- .NET SDK 8 (ou superior, com o targeting pack do `net8.0`)
- MySQL 8 rodando localmente
- Ferramenta `dotnet-ef`:

```bash
dotnet tool install --global dotnet-ef --version 8.0.13
```

## 5. Configuração do MySQL

Crie o banco (as migrations criam as tabelas, mas não o schema):

```sql
CREATE DATABASE InterviewApi CHARACTER SET utf8mb4;
```

Depois preencha a senha em `src/API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=InterviewApi;Uid=root;Pwd=sua_senha;"
  }
}
```

Se o seu servidor não for MySQL 8.0, ajuste também a versão declarada em `src/API/Program.cs`:

```csharp
var serverVersion = new MySqlServerVersion(new Version(8, 0, 46));
```

## 6. Migrations

A migration inicial (`InitialCreate`) já está versionada em `src/Infrastructure/Data/Migrations`.
Para aplicá-la ao banco:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

Para criar novas migrations:

```bash
dotnet ef migrations add NomeDaMigration --project src/Infrastructure --startup-project src/API --output-dir Data/Migrations
```

O projeto `Infrastructure` é onde o `DbContext` vive; o projeto `API` é o startup project,
porque é ele quem carrega a connection string e configura o provider.

## 7. Executando a API

```bash
dotnet run --project src/API
```

O Swagger fica disponível em `/swagger` no ambiente de desenvolvimento.

## 8. Endpoints

| Método | Rota                 | Descrição                     | Respostas       |
|--------|----------------------|-------------------------------|-----------------|
| POST   | `/api/products`      | Cria um produto               | 201, 400        |
| GET    | `/api/products`      | Lista todos os produtos       | 200             |
| GET    | `/api/products/{id}` | Busca um produto por id       | 200, 404        |
| DELETE | `/api/products/{id}` | Remove um produto             | 204, 404        |

## 9. Exemplo de requisição

```bash
curl -X POST http://localhost:5172/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Teclado","price":150.90}'
```

Resposta (`201 Created`):

```json
{
  "id": 1,
  "name": "Teclado",
  "price": 150.90,
  "createdAt": "2026-08-27T14:32:10.123456Z"
}
```
