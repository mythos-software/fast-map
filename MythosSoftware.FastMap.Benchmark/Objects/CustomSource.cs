namespace MythosSoftware.FastMap.Benchmark.Objects;

public sealed class CustomSource
{
    public int Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public BenchmarkOrderStatus Status { get; set; }
}