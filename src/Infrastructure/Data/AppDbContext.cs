// ============================================================================
// CAMADA: Infrastructure
// ============================================================================
// Esse projeto (Infrastructure.csproj) referencia TANTO o Domain quanto
// a Application. E aqui que moram os detalhes tecnicos de verdade: EF
// Core, MySQL, e a implementacao concreta das interfaces que o Domain
// definiu (tipo IProductRepository). Nada fora desse projeto (nem
// Application, nem Domain) sabe COMO os dados sao guardados de fato -
// so a Infrastructure sabe.
// ============================================================================

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

// DbContext e uma classe que vem do pacote NuGet
// Microsoft.EntityFrameworkCore. Ela representa "uma sessao de
// conversa com o banco de dados" - e atraves dela que o EF Core sabe
// montar o SQL, rastrear mudancas nos objetos em memoria, e salvar
// tudo de uma vez quando voce pede.
public class AppDbContext : DbContext
{
    // Esse construtor recebe um DbContextOptions<AppDbContext> - um
    // objeto de configuracao (qual banco usar, qual connection string,
    // etc) que e montado la no Program.cs (camada API) e entregue
    // aqui automaticamente pelo container de Injecao de Dependencia,
    // igual explicado no ProductService.cs. "base(options)" repassa
    // essa configuracao pra classe DbContext (a classe-mae) terminar
    // de se montar por dentro.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<Product> representa a tabela "Products" (ou o nome que
    // for configurado) dentro do banco de dados. E atraves dessa
    // propriedade que o resto do codigo consulta
    // ("_context.Products.Where(...)") ou altera
    // ("_context.Products.Add(...)") os produtos.
    // "Set<Product>()" e um metodo do proprio DbContext que devolve
    // esse DbSet - a propriedade "Products" so serve pra dar um nome
    // bonito e facil de usar pra ele.
    public DbSet<Product> Products => Set<Product>();

    // OnModelCreating e um metodo que o EF Core chama sozinho, UMA
    // VEZ, quando esta montando o "mapa" de como as classes C# viram
    // tabelas no banco. "protected override" quer dizer: esse metodo
    // ja existia na classe-mae (DbContext), e aqui a gente esta
    // REESCREVENDO o comportamento padrao dele (isso se chama
    // "override" - sobrescrever).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Em vez de escrever a configuracao de cada entidade aqui
        // dentro (o que viraria uma bagunca conforme o projeto
        // cresce e ganha mais tabelas), ApplyConfigurationsFromAssembly
        // varre o ASSEMBLY inteiro (assembly = a DLL compilada desse
        // projeto Infrastructure) procurando por qualquer classe que
        // implemente IEntityTypeConfiguration<T> - no nosso caso, so
        // existe a ProductConfiguration (proximo arquivo, na pasta
        // Configurations). Isso aplica a configuracao dela
        // automaticamente, sem precisar listar cada uma na mao aqui.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
