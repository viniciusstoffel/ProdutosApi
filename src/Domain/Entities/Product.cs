namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // decimal evita os erros de arredondamento do ponto flutuante em valores monetários.
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
