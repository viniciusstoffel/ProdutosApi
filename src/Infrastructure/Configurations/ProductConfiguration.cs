using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

// Isso e a "Fluent API" do EF Core - uma forma de configurar como a
// entidade Product vira uma tabela no banco usando uma sequencia de
// chamadas de metodo encadeadas (builder.Algo().OutroAlgo()...), em
// vez de usar attributes [ ] direto na classe Product (o EF Core
// permite isso tambem, mas ai o Domain precisaria ter uma referencia
// ao pacote do EF Core so pra usar os attributes dele - e a gente
// quer o Domain livre de qualquer coisa relacionada a infraestrutura,
// lembra do header explicado em Infrastructure/Data/AppDbContext.cs).
//
// IEntityTypeConfiguration<Product> e uma interface que vem de dentro
// do proprio pacote do EF Core - implementar ela e o "sinal" que o
// ApplyConfigurationsFromAssembly (visto no AppDbContext.cs) procura
// pra saber que essa classe aqui configura a entidade Product.
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // Esse e o unico metodo que a interface pede pra ser implementado.
    // "EntityTypeBuilder<Product>" e o objeto que oferece os metodos
    // de configuracao encadeaveis usados abaixo.
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Nome da tabela dentro do banco de dados.
        builder.ToTable("Products");

        // Diz que a coluna Id e a CHAVE PRIMARIA (Primary Key) - o
        // identificador unico de cada linha dessa tabela. E gracas a
        // isso que o MySQL sabe gerar o AUTO_INCREMENT pra essa
        // coluna (o EF Core percebe sozinho que Id e do tipo int e
        // e chave primaria, entao configura auto-incremento por
        // padrao).
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()        // vira NOT NULL no banco - coluna obrigatoria
            .HasMaxLength(100);  // vira VARCHAR(100) - tamanho maximo da coluna

        builder.Property(p => p.Price)
            .IsRequired()
            // Sem isso, o EF Core (atraves do driver Pomelo) escolheria
            // uma precisao padrao pra coluna decimal, que pode nao ser
            // ideal pra dinheiro. "decimal(18,2)" quer dizer: ate 18
            // digitos no TOTAL, sendo 2 deles depois da virgula. Da
            // pra guardar ate 9999999999999999.99 - mais que
            // suficiente pra qualquer preco realista de produto.
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
