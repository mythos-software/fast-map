namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class OrderDestination
{
    public Guid Id { get; set; }
    
    public string Number { get; set; } = string.Empty;
    
    public OrderCustomerDestination Customer { get; set; } = new();
    
    public List<OrderLineDestination> Lines { get; set; } = [];
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public decimal Total { get; set; }
    
    public BenchmarkOrderStatus Status { get; set; }
}