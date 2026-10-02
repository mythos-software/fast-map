# MythosSoftware.FastMap

FastMap maps .NET objects using profiles, convention-based property mapping, and compiled mapping delegates. It supports custom member expressions, value resolvers, nested objects, collections, and mapping into existing destinations.

[![NuGet](https://img.shields.io/nuget/vpre/MythosSoftware.FastMap)](https://www.nuget.org/packages/MythosSoftware.FastMap/)

## Installation

The current source targets .NET 10. Dependency injection integration is included in the main package; no separate extensions package is needed.

```sh
dotnet add package MythosSoftware.FastMap
```

## Define a profile

Derive from `Profile` and register each source/destination pair with `CreateMap`. Readable source properties are mapped to writable destination properties with matching names. Use `ForMember` to rename, compute, or ignore a destination property.

```csharp
using MythosSoftware.FastMap;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
}

public class PersonDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string InternalNote { get; set; } = "";
}

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Surname))
            .ForMember(dest => dest.InternalNote, opt => opt.Ignore())
            .ReverseMap();
    }
}
```

`Id` is mapped by convention. `Ignore()` preserves the destination property's existing value (or its initial value on a newly created destination).

## Register and use the mapper

Register explicit profile instances:

```csharp
using Microsoft.Extensions.DependencyInjection;
using MythosSoftware.FastMap;

using var provider = new ServiceCollection()
    .AddFastMap(new PersonProfile())
    .BuildServiceProvider();

var mapper = provider.GetRequiredService<IMapper>();
var person = new Person { Id = 1, Name = "Ada", Surname = "Lovelace" };

// Infer the source type from the object's runtime type.
var dto = mapper.Map<PersonDto>(person);

// Supply both types explicitly.
var anotherDto = mapper.Map<Person, PersonDto>(person);

// Map into an existing destination.
var existing = new PersonDto { InternalNote = "Keep this note" };
mapper.Map(person, existing);

// ReverseMap() registers the opposite direction.
var roundTrip = mapper.Map<Person>(dto);
```

Alternatively, discover profiles from assemblies:

```csharp
services.AddFastMap(typeof(PersonProfile).Assembly);
```

Discovered profiles must be concrete classes with parameterless constructors. `IMapper` is registered as a singleton. Register all required object type pairs before mapping; a missing configuration throws `InvalidOperationException`.

## Computed members

`MapFrom` accepts a source expression, not just a property reference:

```csharp
CreateMap<Person, PersonDto>()
    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.ToUpperInvariant()))
    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => $"{src.Name} {src.Surname}"));
```

`ReverseMap()` supports convention mappings and direct property renames. Computed expressions and custom resolvers cannot be automatically reversed and cause `ReverseMap()` to throw. Define a separate reverse profile mapping for those cases.

## Custom value resolvers

Implement `IValueResolver<TSource, TDestination, TMember>` when a member needs access to the source, destination, and current destination member value:

```csharp
using MythosSoftware.FastMap;
using MythosSoftware.FastMap.MappingProcessors;

public class FullNameResolver : IValueResolver<Person, PersonDto, string>
{
    public string Resolve(Person source, PersonDto destination, string currentValue)
    {
        return $"{source.Name} {source.Surname}";
    }
}

public class ResolvedPersonProfile : Profile
{
    public ResolvedPersonProfile()
    {
        CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom<FullNameResolver>());
    }
}
```

Register `ResolvedPersonProfile` instead of `PersonProfile` to use this alternative mapping. The resolver's result is assigned to the selected property, replacing convention-based mapping for that property. When mapping into an existing object, `currentValue` is that property's value before the resolver runs; for a new destination it is the initial value. Convention-based assignments run before configured member assignments.

Resolvers must have a public parameterless constructor. They are created during profile configuration and reused by that mapping, not resolved through dependency injection. Since the mapper is a singleton, resolvers should be safe for concurrent use. Resolver exceptions propagate to the caller.

## Nested objects

Register mappings for nested object types as well as their containing types. Matching nested properties are mapped through the same mapper:

```csharp
public class Address
{
    public string City { get; set; } = "";
}

public class AddressDto
{
    public string City { get; set; } = "";
}

public class Customer
{
    public Address? Address { get; set; }
}

public class CustomerDto
{
    public AddressDto? Address { get; set; }
}

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Address, AddressDto>();
        CreateMap<Customer, CustomerDto>();
    }
}
```

A null source property sets the matching destination property to its default value, unless that member is ignored or has custom configuration.

## Collections

Collections are handled automatically; register the element mappings rather than a collection mapping:

```csharp
var people = new List<Person>
{
    new() { Id = 1, Name = "Ada", Surname = "Lovelace" }
};

var list = mapper.Map<List<PersonDto>>(people);
var array = mapper.Map<PersonDto[]>(people);
var sequence = mapper.Map<IEnumerable<PersonDto>>(people);

var existingList = new List<PersonDto>();
mapper.Map(people, existingList);
```

Supported shapes include arrays, lists, enumerable interfaces, sets, queues, and dictionaries. Dictionary keys and values use their respective registered mappings when needed. Mapping into mutable collections adds items without clearing existing contents; existing dictionary keys can therefore cause duplicate-key errors. Arrays and other destinations that cannot be updated in place are replaced, so use the return value of `Map` when mapping into those destinations.
