using Domain.Entities;

namespace Domain.Interfaces;

/*
 * Interface = contrato, sem implementacao. Quem implementa de verdade
 * e a ProductRepository, la na Infrastructure - o Domain nao sabe
 * que essa classe existe.
 *
 * Task<T>: trabalho assincrono que devolve T quando terminar (use await).
 * IEnumerable<T>: uma sequencia percorrivel de T, mais generico que List/array.
 * CancellationToken: sinal pra cancelar a operacao se o cliente desistir
 * da requisicao no meio do caminho.
 */
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    // Product? pode devolver null se o id nao existir
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
