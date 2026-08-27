// Continua na camada Domain - ainda sem nenhuma referencia a outros
// projetos da solucao. Veja o header completo em Entities/Product.cs.

using Domain.Entities;

namespace Domain.Interfaces;

// Uma INTERFACE em C# e um "contrato": ela lista quais metodos devem
// existir (nome, parametros, o que devolve), mas NAO tem o codigo de
// como cada metodo funciona por dentro - isso fica por conta de quem
// implementa ela em outro lugar.
//
// Por convencao, toda interface em C# comeca com "I" maiusculo
// (IProductRepository, IProductService, etc) - isso e so um apelido
// visual pra facilitar reconhecer "isso e uma interface" olhando o
// nome, nao muda em nada o comportamento do codigo.
//
// Aqui a gente define O QUE significa "acessar produtos guardados em
// algum lugar", sem dizer ONDE esse lugar e (pode ser MySQL, pode ser
// uma lista em memoria usada num teste, pode ser qualquer coisa).
// Quem implementa de verdade e a classe ProductRepository, la na
// camada Infrastructure (arquivo Infrastructure/Repositories/ProductRepository.cs)
// - mas o Domain nao sabe que essa classe existe, e nao precisa saber.
public interface IProductRepository
{
    // Task<T> representa "um trabalho assincrono que, quando terminar,
    // vai entregar um valor do tipo T". Operacoes de banco de dados
    // (chamadas de IO - "entrada/saida", tipo rede ou disco) demoram
    // um tempo que nao da pra prever. Em vez de travar (bloquear) a
    // aplicacao inteira esperando a resposta, o metodo devolve uma
    // especie de "promessa" (a Task) que sera completada quando o
    // banco responder. Quem chama esse metodo usa a palavra "await"
    // pra esperar o resultado sem travar o programa inteiro nesse meio
    // tempo (isso fica mais claro olhando o ProductService.cs).
    //
    // IEnumerable<T> significa "uma sequencia de itens do tipo T que
    // da pra percorrer com um foreach". E mais generico que List<T> ou
    // T[] (array) - ele so garante que da pra iterar item por item,
    // nao garante que da pra acessar por indice (tipo lista[0]) nem
    // saber o tamanho de cara. Aqui, IEnumerable<Product> = "uma
    // lista de produtos, de algum tipo, que da pra percorrer".
    //
    // CancellationToken e um "sinal de cancelamento". Imagina que o
    // usuario fechou a aba do navegador no meio de uma requisicao
    // HTTP demorada - o ASP.NET Core sabe disso e "avisa" atraves
    // desse token, e o codigo (se estiver escutando o token) pode
    // parar de trabalhar mais cedo em vez de gastar tempo e
    // processamento pra um resultado que ninguem mais vai receber.
    // Aqui a gente so passa ele adiante pro EF Core, que ja sabe
    // usar esse sinal sozinho por dentro.
    //
    // "= default" quer dizer que esse parametro e OPCIONAL - se quem
    // chamar o metodo nao passar nada, ele recebe o valor padrao do
    // tipo (pra CancellationToken, "default" e um token que nunca
    // e cancelado, ou seja, "nao me avise de nada").
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    // Product? (com um ponto de interrogacao logo depois do tipo)
    // quer dizer "isso pode ser um Product OU pode ser null". Esse e
    // o recurso de "nullable reference types" do C# moderno - o
    // compilador te avisa em tempo de compilacao se voce esquecer de
    // checar por null antes de usar o valor. Faz todo sentido aqui:
    // pode ser que o Id pedido nao exista no banco, e nesse caso o
    // metodo devolve null em vez de dar erro.
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Esse Task (sem <T> do lado) significa "um trabalho assincrono
    // que nao devolve nenhum valor quando termina" - so um "ok, ja
    // terminei". E o equivalente assincrono de um metodo void.
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
