namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class OrderLineDestination
{
    public Guid ProductId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
}