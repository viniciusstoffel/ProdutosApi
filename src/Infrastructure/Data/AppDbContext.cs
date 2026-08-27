/*
 * Camada Infrastructure. Referencia Domain e Application - é aqui que
 * moram os detalhes técnicos (EF Core, MySQL) e a implementação real
 * das interfaces que o Domain definiu.
 */

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

// DbContext = sessão de conversa com o banco, do pacote EF Core
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<Product> = a tabela Products, acessada por aqui
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // varre o projeto procurando classes IEntityTypeConfiguration<T>
        // (só existe a ProductConfiguration por enquanto)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
