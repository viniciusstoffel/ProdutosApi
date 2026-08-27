/*
 * Camada Domain. Não referencia nenhum outro projeto da solução - não
 * conhece Application, Infrastructure ou API, nem sabe que EF Core
 * existe. Isso permite trocar banco/framework sem tocar aqui.
 */

namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // decimal, não double/float - precisão exata pra dinheiro
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
