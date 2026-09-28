# MythosSoftware.FastMap

`MythosSoftware.FastMap` will help you when mapping objects.

[![NuGet](https://img.shields.io/nuget/vpre/MythosSoftware.FastMap)](https://www.nuget.org/packages/MythosSoftware.FastMap/)

## Configuration examples

- Usage together with Dependency injection and FastMap.Extensions.Microsoft.DependencyInjection package
```	
var serviceProvider = new ServiceCollection()
    .AddFastMap(Assembly.GetExecutingAssembly())
    .BuildServiceProvider();

```

How to get it
--------------------------------
Use NuGet Package Manager to install the package or use any of the following commands in NuGet Package Manager Console.

```	
PM> Install-Package MythosSoftware.FastMap
```