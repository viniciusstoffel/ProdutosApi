/*
 * Camada Domain. Nao referencia nenhum outro projeto da solucao - nao
 * conhece Application, Infrastructure ou API, nem sabe que EF Core
 * existe. Isso permite trocar banco/framework sem tocar aqui.
 */

namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // decimal, nao double/float - precisao exata pra dinheiro
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
