using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// Aqui, finalmente, a implementacao DE VERDADE de IProductRepository
// (que ate agora era so um contrato, la no Domain). Repare no
// "class ProductRepository : IProductRepository" - esse ":" aqui
// significa "essa classe implementa essa interface", ou seja, ela e
// OBRIGADA a ter todos os metodos que a interface prometeu, com a
// mesma assinatura (mesmo nome, mesmos parametros, mesmo retorno). Se
// faltar algum metodo, o codigo nem compila.
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    // De novo, injecao via construtor - o container de DI (configurado
    // no Program.cs) entrega um AppDbContext ja pronto quando cria
    // esse ProductRepository, sem que a gente precise escrever
    // "new AppDbContext(...)" em lugar nenhum.
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking() avisa o EF Core: "essa consulta e so
        // leitura, eu nao vou editar esses objetos e salvar de volta,
        // entao nao precisa gastar memoria e processamento
        // rastreando mudanca neles" (o EF Core, por padrao, fica de
        // olho em cada objeto carregado pra saber o que mudou quando
        // voce chama SaveChanges - aqui a gente desliga isso de
        // proposito, por performance, ja que e um GET que so le).
        return await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt) // mais recentes primeiro
            .ToListAsync(cancellationToken); // executa a query no banco e traz tudo como List
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // FindAsync busca por CHAVE PRIMARIA - e o jeito mais rapido
        // de buscar um registro por Id, porque o EF Core sabe
        // otimizar esse caso especifico (inclusive olhando primeiro
        // se esse objeto ja esta em memoria, antes de ir ate o banco
        // de novo).
        //
        // "[id]" aqui NAO e um attribute (diferente dos [Required]
        // que vimos no CreateProductDto.cs, mesmo usando o mesmo
        // simbolo de colchetes) - aqui e uma COLLECTION EXPRESSION
        // (recurso do C# 12), um jeito curto de escrever um array.
        // FindAsync espera receber um array de valores de chave,
        // porque em tabelas com CHAVE COMPOSTA (mais de uma coluna
        // formando a chave primaria junto) precisaria de mais de um
        // valor dentro desse array. No nosso caso e so o Id, entao o
        // array tem um item so. Antes do C# 12, isso seria escrito
        // "new object[] { id }" - e exatamente o mesmo resultado, so
        // que mais verboso de digitar.
        return await _context.Products.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        // AddAsync marca o objeto como "novo, precisa ser inserido" -
        // mas AINDA NAO manda nada pro banco nesse momento.
        await _context.Products.AddAsync(product, cancellationToken);
        // E so aqui, no SaveChangesAsync, que o EF Core de fato monta
        // o SQL (um INSERT INTO...) e manda pro MySQL executar. Ele
        // junta TODAS as mudancas pendentes (poderia ter varios
        // Add/Remove acumulados ao mesmo tempo) e manda tudo de uma
        // vez so, numa unica transacao no banco.
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
