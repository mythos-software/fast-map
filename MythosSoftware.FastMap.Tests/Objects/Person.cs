namespace MythosSoftware.FastMap.Tests.Objects;

public class SimplePerson
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
}

public class ModifiedSimplePerson
{
    public int Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
}

public class SimpleDestinationPerson : SimplePerson {}

public class SubclassPerson
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public SimpleHobby Hobby { get; set; }
}

public class SubclassDestinatonPerson : SubclassPerson {}

public class NullablePerson
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string? Address { get; set; }
}

public class NullableDestinationPerson : NullablePerson {}

public class StringCollectionPerson
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public List<string> Hobbies { get; set; }
}

public class StringCollectionDestinationPerson : StringCollectionPerson {}

public class CollectionPerson
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public List<SimpleHobby> Hobbies { get; set; }
}

public class CollectionDestinationPerson : CollectionPerson {}