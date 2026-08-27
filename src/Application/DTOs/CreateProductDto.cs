/*
 * Camada Application. So referencia o Domain - nao conhece a
 * Infrastructure, nao sabe que existe banco de dados. Aqui ficam os
 * DTOs (formato de entrada/saida da API) e a logica de aplicacao
 * (ProductService).
 */

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

/*
 * DTO = Data Transfer Object, so carrega dado, sem logica. Diferente
 * da entidade Product porque nao tem Id nem CreatedAt - esses dois sao
 * gerados pelo servidor, nao faz sentido o cliente enviar.
 */
public class CreateProductDto
{
    // [Required]/[MaxLength]/[Range] sao attributes - o ASP.NET Core
    // valida isso sozinho antes do Controller rodar, sem if manual
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Price { get; set; }
}
