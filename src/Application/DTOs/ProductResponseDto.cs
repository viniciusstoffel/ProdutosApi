namespace Application.DTOs;

// DTO de saída: a entidade de domínio não é exposta diretamente pela API.
public class ProductResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
