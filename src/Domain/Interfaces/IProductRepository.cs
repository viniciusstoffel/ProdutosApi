using Domain.Entities;

namespace Domain.Interfaces;

/*
 * Interface = contrato, sem implementação. Quem implementa de verdade
 * é a ProductRepository, lá na Infrastructure - o Domain não sabe
 * que essa classe existe.
 *
 * Task<T>: trabalho assíncrono que devolve T quando terminar (use await).
 * IEnumerable<T>: uma sequência percorrível de T, mais genérico que List/array.
 * CancellationToken: sinal pra cancelar a operação se o cliente desistir
 * da requisição no meio do caminho.
 */
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    // Product? pode devolver null se o id não existir
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
