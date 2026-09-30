using BenchmarkDotNet.Running;
using MythosSoftware.FastMap.Benchmark.Benchmarks;

namespace MythosSoftware.FastMap.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<FastMapBenchmarks>();
    }
}