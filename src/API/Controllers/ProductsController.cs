// ============================================================================
// Ainda na camada API. Esse Controller e o UNICO lugar do projeto que
// "fala HTTP" de verdade - recebe a requisicao, chama a camada
// Application, e devolve uma resposta HTTP. Repare que ele so importa
// Application.DTOs e Application.Interfaces - NUNCA Domain nem
// Infrastructure diretamente. Isso significa que o Controller nao sabe
// (e nao precisa saber) que existe MySQL, EF Core, ou qualquer detalhe
// de como os dados sao guardados - ele so conversa com o "contrato"
// (IProductService).
//
// Vantagem pratica disso: se um dia a forma de guardar produtos mudar
// completamente (trocar MySQL por outra coisa, por exemplo), ESSE
// ARQUIVO AQUI nao precisa mudar nem uma linha - so a Infrastructure
// muda. E tambem fica mais facil testar o Controller sozinho no
// futuro (dando um IProductService falso/fake pra ele em um teste
// automatizado), sem precisar de um banco de dados de verdade rodando
// so pra rodar o teste.
// ============================================================================

using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// [ApiController] e um ATTRIBUTE (metadado, igual os [Required] que
// vimos no CreateProductDto.cs) que liga varios comportamentos
// automaticos pensados pra Web API: validacao automatica do modelo
// recebido (se o DTO nao bater com as regras tipo [Required], o
// ASP.NET Core ja devolve 400 sozinho, sem voce escrever if pra
// isso), leitura automatica do corpo JSON pros parametros do metodo,
// entre outras conveniencias.
[ApiController]
// [Route("api/[controller]")] define o "endereco base" desse
// Controller. O "[controller]" AQUI DENTRO DA STRING e um PLACEHOLDER
// especial do ASP.NET Core (nao e um attribute separado, e so um
// textinho magico dentro da rota) que e substituido automaticamente
// pelo nome da classe SEM o sufixo "Controller" - ou seja,
// "ProductsController" vira "Products", entao a rota final fica
// "api/Products" (e o ASP.NET Core nao diferencia maiusculas de
// minusculas nas rotas, entao "api/products" tambem funciona igual).
[Route("api/[controller]")]
// ControllerBase e a classe-mae que da acesso a metodos prontos tipo
// Ok(), NotFound(), CreatedAtAction(), NoContent() (usados abaixo)
// pra montar respostas HTTP com o codigo de status certo, sem
// precisar montar a resposta na mao.
public class ProductsController : ControllerBase
{
    // O controller depende apenas da camada Application: nada de
    // DbContext ou repositorio aqui, so o contrato IProductService.
    private readonly IProductService _productService;

    // De novo, injecao via construtor - o ASP.NET Core cria esse
    // Controller automaticamente pra cada requisicao HTTP que chega
    // em /api/products, e entrega um IProductService ja pronto (que,
    // por baixo dos panos, e um ProductService com um
    // IProductRepository dentro, que por sua vez tem um AppDbContext
    // dentro - a cadeia inteira montada la no Program.cs).
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // [HttpGet] diz: esse metodo responde requisicoes HTTP do tipo
    // GET, na rota base do Controller (api/Products).
    [HttpGet]
    // [ProducesResponseType(...)] e so DOCUMENTACAO pro Swagger -
    // avisa "esse endpoint pode devolver esse tipo de dado, com esse
    // codigo de status" pra telinha do Swagger mostrar certinho pra
    // quem for testar. Nao muda nada no comportamento em tempo de
    // execucao, so enriquece a documentacao gerada automaticamente.
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    // ActionResult<T> e um tipo especial que permite o metodo
    // devolver OU um objeto do tipo T (que vira 200 OK com esse JSON
    // no corpo da resposta), OU um resultado de acao tipo
    // NotFound()/BadRequest() (que ja tem seu proprio codigo de
    // status). E o tipo de retorno mais comum pra endpoints de
    // Controller que podem responder de mais de um jeito dependendo
    // do que acontecer.
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        // Ok(...) monta uma resposta HTTP 200 com esse objeto
        // serializado (convertido) automaticamente pra JSON no corpo
        // da resposta.
        return Ok(products);
    }

    // "{id:int}" AQUI e diferente dos colchetes [ ] dos attributes -
    // isso e um ROUTE TEMPLATE (modelo de rota), escrito com CHAVES
    // { }. O "{id" captura o pedaco da URL que estiver naquela
    // posicao e guarda numa variavel chamada "id" (que precisa bater
    // com o nome do parametro do metodo, "int id" logo abaixo). O
    // ":int" depois dos dois-pontos e uma RESTRICAO DE ROTA (route
    // constraint) - so aceita cair nesse endpoint se o pedaco da URL
    // for de fato um numero inteiro; se alguem acessar
    // "/api/products/abc" (uma letra em vez de numero), o ASP.NET
    // Core nem chega a chamar esse metodo, ja devolve 404 sozinho
    // antes disso.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        // Operador ternario de novo: se product for null, devolve
        // NotFound() (que vira HTTP 404); senao, devolve Ok(product)
        // (que vira HTTP 200 com o JSON do produto).
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Create(
        // O ASP.NET Core sabe automaticamente que esse parametro
        // "dto" deve vir do CORPO (body) da requisicao HTTP, em
        // formato JSON, e faz essa conversao (texto JSON -> objeto
        // C#) sozinho, gracas ao [ApiController] la em cima da
        // classe. Em versoes mais antigas do ASP.NET Core era preciso
        // escrever [FromBody] na frente do parametro pra deixar isso
        // explicito - com [ApiController], isso e inferido (deduzido)
        // automaticamente pelo framework.
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        // Repare que NAO tem nenhum "if (!ModelState.IsValid)" aqui.
        // Isso e porque [ApiController] ja faz essa checagem
        // SOZINHO, antes mesmo desse metodo comecar a executar - se
        // o dto recebido nao bater com as regras (Required, MaxLength,
        // Range, vistas no CreateProductDto.cs), a requisicao ja para
        // e devolve 400 Bad Request automaticamente, sem chegar nem
        // na primeira linha desse metodo aqui.
        var product = await _productService.CreateAsync(dto, cancellationToken);

        // CreatedAtAction monta a resposta correta pra quando voce
        // CRIA um recurso novo via POST: devolve 201 Created (nao
        // 200 OK - o 201 especificamente significa "algo novo foi
        // criado com sucesso"), e preenche o cabecalho HTTP
        // "Location" com a URL onde esse produto pode ser consultado
        // depois (apontando pro metodo GetById, com esse id) - isso e
        // uma convencao do padrao REST pra recursos recem-criados.
        // "nameof(GetById)" pega o NOME do metodo GetById como texto
        // ("GetById") de um jeito seguro - se voce renomear o metodo
        // GetById no futuro, o nameof atualiza sozinho junto,
        // diferente de escrever a string "GetById" na mao (que
        // quebraria silenciosamente se voce renomeasse o metodo e
        // esquecesse de atualizar essa string em algum outro lugar).
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // IActionResult (sem o <T> do lado) e usado aqui porque nenhuma
    // das duas respostas possiveis (NoContent ou NotFound) carrega um
    // corpo/JSON de verdade pra devolver - entao nao precisa do
    // generico <T> do ActionResult<T>, que so faz sentido quando tem
    // um objeto real sendo devolvido no corpo da resposta.
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        // NoContent() = HTTP 204 (deu certo, mas nao tem nada pra
        // devolver no corpo da resposta - faz todo sentido pra um
        // DELETE que teve sucesso, ja que nao sobrou nenhum dado pra
        // mostrar sobre algo que acabou de ser apagado).
        return deleted ? NoContent() : NotFound();
    }
}
