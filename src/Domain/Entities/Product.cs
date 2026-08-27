// ============================================================================
// CAMADA: Domain
// ============================================================================
// Esse projeto (Domain.csproj) NAO referencia nenhum outro projeto da solucao.
// Ele nao conhece Application, nao conhece Infrastructure, nao conhece API.
// Ele nem sabe que existe Entity Framework, MySQL ou ASP.NET Core - e so
// C# puro, sem nenhum pacote NuGet de infraestrutura.
//
// Isso e proposital: a ideia da Clean Architecture e que a regra de negocio
// (o que e um Produto, quais campos ele tem) nao deveria mudar so porque
// voce trocou de banco de dados ou de framework web. Se um dia trocar
// MySQL por PostgreSQL, esse arquivo aqui nao muda uma linha.
// ============================================================================

namespace Domain.Entities;

// "Entity" (entidade) e o nome que se da, em Domain-Driven Design, pra uma
// classe que representa "uma coisa do mundo real" que o sistema controla,
// e que tem uma identidade unica (aqui, o Id). Dois produtos com o mesmo
// Name e Price mas Id diferente sao PRODUTOS DIFERENTES.
public class Product
{
    // "get; set;" cria uma PROPRIEDADE auto-implementada. E um jeito curto
    // de escrever "um campo guardado + um metodo pra ler ele (get) + um
    // metodo pra escrever nele (set)", sem voce ter que escrever os dois
    // na mao. Por fora, voce usa como se fosse uma variavel normal:
    // produto.Id = 5;  ou  var x = produto.Id;
    public int Id { get; set; }

    // string.Empty e o mesmo que "" (string vazia), mas e considerado
    // mais idiomatico em C#. O "= string.Empty" aqui e um VALOR PADRAO:
    // se ninguem preencher o Name explicitamente, ele comeca como ""
    // em vez de null. Isso evita erros de "null reference" (tentar usar
    // algo que nao existe) mais pra frente no codigo.
    public string Name { get; set; } = string.Empty;

    // decimal e um tipo numerico do .NET feito especificamente pra
    // dinheiro/valores financeiros. Ele guarda o numero de forma EXATA.
    // double e float (os outros tipos numericos com casas decimais)
    // guardam o numero de forma APROXIMADA por dentro (ponto flutuante
    // binario), entao contas como 0.1 + 0.2 podem dar
    // 0.30000000000000004 em vez de 0.3 exato. Pra dinheiro isso e
    // inaceitavel, entao sempre use decimal, nunca double/float, pra
    // preco ou qualquer valor monetario.
    public decimal Price { get; set; }

    // DateTime e o tipo do .NET pra representar data + hora junto.
    // Repare que aqui a gente NAO usa "= DateTime.Now" nem nada
    // parecido - quem decide o valor desse campo e o ProductService
    // (la na camada Application, arquivo ProductService.cs), nao a
    // entidade. A entidade so guarda o dado, ela nao tem logica de
    // "quando preencher isso".
    public DateTime CreatedAt { get; set; }
}
