using Domain.Entities;

namespace Domain.Interfaces;

// A abstração vive no Domain e é implementada na Infrastructure:
// é isso que mantém a dependência apontando para dentro (Dependency Inversion).
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
