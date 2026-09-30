namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class OrderSource
{
    public Guid Id { get; set; }
    
    public string Number { get; set; } = string.Empty;
    
    public OrderCustomerSource Customer { get; set; } = new();
    
    public List<OrderLineSource> Lines { get; set; } = [];
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public decimal Total { get; set; }
    
    public BenchmarkOrderStatus Status { get; set; }
}