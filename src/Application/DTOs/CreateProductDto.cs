// ============================================================================
// CAMADA: Application
// ============================================================================
// Esse projeto (Application.csproj) so referencia o Domain (existe um
// ProjectReference pro Domain.csproj, e nenhum outro). Ele NAO conhece a
// Infrastructure - nao sabe que existe EF Core, nao sabe que existe MySQL,
// nao sabe como os dados sao guardados de verdade. Ele so conhece as
// INTERFACES que o Domain definiu (tipo IProductRepository) e trabalha
// com elas as cegas, sem saber quem implementa por baixo.
//
// E aqui que fica a "logica de aplicacao": o que o sistema FAZ quando
// alguem pede pra criar/listar/buscar/deletar um produto. Tambem e aqui
// que ficam os DTOs (explicado abaixo) - o formato de dado que entra e
// sai pela API, que e DIFERENTE da entidade Product do Domain.
//
// Se voce ainda ta com duvida sobre "pra que serve a Application", da
// uma olhada na secao 14 do GUIA-COMPLETO.md - tem um fluxograma la.
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

// DTO = Data Transfer Object (objeto de transferencia de dados).
// E uma classe "burra", so com propriedades, cujo unico trabalho e
// carregar dados de um lugar pro outro - nesse caso, do JSON que chega
// no corpo (body) de um POST ate o codigo C#.
//
// Por que nao usar a entidade Product direto aqui, ja que ela ja tem
// Name e Price? Porque Product tambem tem Id e CreatedAt, e esses dois
// campos NAO fazem sentido o cliente (quem chama a API) enviar: o Id
// e gerado pelo banco, e o CreatedAt e definido pelo servidor no
// momento da criacao. Se a API usasse Product direto como "formato de
// entrada", um cliente mal-intencionado poderia mandar um CreatedAt
// inventado la do passado, por exemplo. O DTO evita isso: ele so tem
// os campos que realmente fazem sentido o cliente mandar.
//
// Essa e a diferenca chave entre DTO e a Infrastructure: o DTO e um
// formato que so existe NA FRONTEIRA entre o mundo de fora (HTTP/JSON)
// e a Application. A Infrastructure NUNCA VE um DTO - ela so trabalha
// com a entidade Product (do Domain). Quem faz essa "traducao" de DTO
// pra entidade e o ProductService, no meio do caminho.
public class CreateProductDto
{
    // Isso aqui, entre colchetes [ ], e um ATTRIBUTE (atributo) do C#.
    // Atributo NAO tem nada a ver com array/lista, apesar de usar o
    // mesmo simbolo [ ] - e uma forma de anexar METADADO (informacao
    // extra) numa classe, propriedade ou metodo, que o proprio .NET
    // (ou uma biblioteca) le em tempo de execucao pra mudar o
    // comportamento sem voce escrever codigo manual pra isso.
    //
    // [Required] e um "Data Annotation" (anotacao de dados) que diz:
    // "esse campo e obrigatorio". O ASP.NET Core, antes mesmo do seu
    // codigo no Controller rodar, ja confere sozinho se o JSON
    // recebido tem esse campo preenchido - se nao tiver, ele ja
    // responde 400 Bad Request pro cliente automaticamente, sem voce
    // escrever nenhum "if (name == null)" na mao.
    [Required]
    // [MaxLength(100)] e outro Data Annotation: limita o tamanho
    // maximo da string em 100 caracteres. Mesma ideia - validacao
    // automatica, sem codigo manual.
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // [Range(min, max)] valida que o numero esta dentro de um
    // intervalo. Aqui: entre 0.01 (um centavo) e o maior double
    // possivel (usamos double.MaxValue como limite superior porque o
    // atributo RangeAttribute do .NET exige um double/int como
    // parametro, mesmo a propriedade Price sendo decimal - e uma
    // limitacao de como esse attribute especifico foi construido pela
    // Microsoft, nao um erro nosso).
    // ErrorMessage e o texto que volta pro cliente se a validacao falhar.
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Price { get; set; }
}
