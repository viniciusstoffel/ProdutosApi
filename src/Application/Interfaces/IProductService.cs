using Application.DTOs;

namespace Application.Interfaces;

// Igual o IProductRepository (que fica no Domain), essa e uma
// interface - um contrato sem implementacao. A diferenca de
// responsabilidade entre os dois:
//
//   IProductRepository (Domain)   -> "como eu leio/escrevo produtos
//                                     no lugar onde eles sao
//                                     guardados?" (so operacoes de
//                                     dados, tipo um CRUD basico -
//                                     Create, Read, Update, Delete)
//
//   IProductService (Application) -> "o que o sistema FAZ quando
//                                     alguem PEDE pra criar/listar um
//                                     produto?" (regra de aplicacao -
//                                     pode envolver mais de uma
//                                     operacao de dados, conversao
//                                     pra DTO, decisoes tipo "qual
//                                     data eu uso", etc)
//
// Repare que os metodos daqui trabalham com DTOs (CreateProductDto,
// ProductResponseDto), nao com a entidade Product do Domain
// diretamente. Isso e proposital: quem usa essa interface (o
// Controller, la na camada API) nunca precisa importar nada do
// Domain - ele so conhece Application.
public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductResponseDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    // bool aqui e usado como "deu certo ou nao": true se o produto
    // existia e foi deletado, false se o Id pedido nao foi encontrado.
    // E uma forma simples do Controller saber se deve responder 204
    // (sucesso, sem conteudo) ou 404 (nao encontrado) depois.
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
