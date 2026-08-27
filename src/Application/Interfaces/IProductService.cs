using Application.DTOs;

namespace Application.Interfaces;

/*
 * IProductRepository (Domain) = como ler/escrever no banco.
 * IProductService (Application) = o que fazer quando alguem pede pra
 * criar/listar um produto - trabalha com DTO, nao com a entidade direto.
 */
public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponseDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
