using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MythosSoftware.FastMap.Tests.Providers;

public class TestServiceProvider
{
    public ServiceProvider CreateServices()
    {
        var serviceProvider = new ServiceCollection()
            .AddFastMap(Assembly.GetExecutingAssembly())
            .BuildServiceProvider();

        return serviceProvider;
    }
}