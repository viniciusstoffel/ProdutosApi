namespace Application.DTOs;

// DTO de saida - tem Id e CreatedAt porque agora faz sentido o cliente
// receber esses dados de volta
public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
