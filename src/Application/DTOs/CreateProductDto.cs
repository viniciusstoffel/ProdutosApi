/*
 * Camada Application. Só referencia o Domain - não conhece a
 * Infrastructure, não sabe que existe banco de dados. Aqui ficam os
 * DTOs (formato de entrada/saída da API) e a lógica de aplicação
 * (ProductService).
 */

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

/*
 * DTO = Data Transfer Object, só carrega dado, sem lógica. Diferente
 * da entidade Product porque não tem Id nem CreatedAt - esses dois são
 * gerados pelo servidor, não faz sentido o cliente enviar.
 */
public class CreateProductDto
{
    // [Required]/[MaxLength]/[Range] são attributes - o ASP.NET Core
    // valida isso sozinho antes do Controller rodar, sem if manual
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Price { get; set; }
}
