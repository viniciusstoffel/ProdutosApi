namespace Application.DTOs;

// Esse e o DTO de SAIDA - o formato exato do JSON que a API devolve pro
// cliente depois de criar/buscar um produto. Repare que ele tem TODOS
// os campos (incluindo Id e CreatedAt), diferente do CreateProductDto
// (que so tem Name e Price). Nesse sentido (servidor -> cliente) faz
// todo sentido informar Id e CreatedAt - o cliente PRECISA saber qual
// Id foi gerado pro produto novo, por exemplo, pra poder buscar ele
// de novo depois.
//
// Ter DTOs separados pra entrada (CreateProductDto) e pra saida
// (ProductResponseDto) e um padrao comum: nem sempre o formato que
// entra e igual ao que sai. Aqui os dois ficam parecidos, mas em
// sistemas maiores essa diferenca fica bem mais evidente (por
// exemplo, um DTO de saida pode incluir campos calculados, ou
// esconder campos sensiveis que existem na entidade mas nao deveriam
// ser expostos por HTTP).
public class ProductResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
