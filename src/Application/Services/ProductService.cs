using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

// Essa e a IMPLEMENTACAO de IProductService - o "cerebro" de verdade
// da aplicacao. E aqui que a conversa entre "DTO que veio da API" e
// "entidade Product que vai pro banco" acontece de verdade.
public class ProductService : IProductService
{
    // "private readonly" - private quer dizer que so o codigo de
    // DENTRO dessa classe pode acessar esse campo (nada de fora
    // consegue fazer productService._repository). readonly quer dizer
    // que o valor so pode ser definido UMA VEZ, dentro do construtor -
    // depois disso ele nao muda mais durante a vida do objeto. E uma
    // boa pratica pra campos que guardam uma dependencia (tipo esse
    // repositorio) que nao deveria trocar no meio do caminho.
    private readonly IProductRepository _repository;

    // Isso e um CONSTRUTOR - o metodo especial que roda quando alguem
    // cria um "new ProductService(...)". Repare que ninguem, em
    // lugar nenhum do nosso codigo, escreve literalmente
    // "new ProductService(...)". Quem cria essa instancia e o
    // container de Injecao de Dependencia do ASP.NET Core, configurado
    // no Program.cs (camada API) - ele ve que esse construtor pede um
    // IProductRepository, e entrega automaticamente a implementacao
    // concreta (ProductRepository, la da Infrastructure) que foi
    // registrada la. Isso e "constructor injection" (injecao via
    // construtor) - a forma mais comum de Injecao de Dependencia em
    // projetos .NET. O passo a passo completo de como isso acontece
    // esta comentado no Program.cs.
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    // "async" no cabecalho do metodo permite usar a palavra "await"
    // dentro dele. "await" pausa a execucao DESSE metodo especifico
    // (sem travar a aplicacao inteira) ate a Task terminar, e ai
    // continua dali com o resultado em maos. E o jeito moderno de C#
    // lidar com operacoes demoradas (banco de dados, chamadas de
    // rede, arquivos) sem precisar travar uma thread inteira so
    // esperando sem fazer nada.
    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);

        // .Select(...) e um metodo de LINQ (Language Integrated Query -
        // um jeito de fazer consultas e transformacoes em colecoes
        // usando sintaxe normal de C#, em vez de escrever loops na
        // mao). Aqui ele pega cada Product da lista e transforma
        // (mapeia) num ProductResponseDto, usando o metodo
        // MapToResponse la de baixo. Passar "MapToResponse" direto
        // (sem parenteses) e chamado de GROUP DE METODO (method group)
        // - da no mesmo que escrever a versao mais explicita
        // "products.Select(p => MapToResponse(p))", onde "p => algo"
        // e uma LAMBDA (uma funcao anonima e curta - o "=>" le-se
        // "vai para" ou "mapeia para").
        return products.Select(MapToResponse);
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);

        // Isso e um OPERADOR TERNARIO: "condicao ? valorSeVerdadeiro : valorSeFalso".
        // Le-se: "se product for null, devolve null; senao, devolve
        // MapToResponse(product)". E um jeito curto de escrever um
        // if/else simples numa linha so.
        return product is null ? null : MapToResponse(product);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        // Aqui a gente pega o DTO (que veio da API, so com Name e
        // Price) e monta a ENTIDADE Product de verdade, que e o tipo
        // que o repositorio (e por baixo dele, o EF Core) sabe salvar
        // no banco. Essa "traducao" de DTO pra entidade e um dos
        // trabalhos centrais da camada Application.
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            // DateTime.UtcNow pega a data/hora atual em UTC (tempo
            // universal coordenado, sem fuso horario aplicado).
            // Usamos UtcNow em vez de DateTime.Now (que usa o fuso
            // horario configurado na maquina onde o codigo roda) pra
            // evitar bugs quando o servidor e os usuarios estao em
            // fusos diferentes - UTC e sempre o mesmo valor, nao
            // importa em qual pais/fuso o servidor esta rodando.
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(product, cancellationToken);

        // Depois do AddAsync, o EF Core ja preencheu o campo Id de
        // volta dentro do objeto "product" (o banco gerou um Id novo
        // via AUTO_INCREMENT e o EF Core "le" esse valor de volta pra
        // dentro do objeto que esta em memoria). Por isso da pra
        // devolver o Id certinho aqui embaixo, sem precisar buscar
        // o produto de novo no banco.
        return MapToResponse(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        await _repository.DeleteAsync(product, cancellationToken);
        return true;
    }

    // "private static" - private (so essa classe usa esse metodo) e
    // static (esse metodo nao depende de nenhuma instancia especifica
    // de ProductService pra funcionar - e so uma funcao utilitaria,
    // poderia ser chamada mesmo sem ter um "ProductService" criado).
    // Ele converte uma entidade Product (formato interno, do Domain)
    // num ProductResponseDto (formato externo, que vira JSON).
    //
    // "=> new() { ... }" e um EXPRESSION-BODIED METHOD (metodo de
    // corpo-expressao): quando o metodo inteiro e uma unica expressao,
    // da pra escrever ele com "=>" em vez de chaves e um "return"
    // explicito. E o "new()" sozinho (sem escrever de novo
    // "new ProductResponseDto()") e chamado de TARGET-TYPED NEW -
    // o compilador ja sabe, pelo tipo de retorno declarado do metodo,
    // qual tipo voce quer criar, entao nao precisa repetir o nome.
    private static ProductResponseDto MapToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        CreatedAt = product.CreatedAt
    };
}
