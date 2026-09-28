namespace MythosSoftware.FastMap.Tests.Objects;

public class SimpleHobby
{
    public int Id { get; set; }
    
    public string Name { get; set; }
}

public class SimpleDestinationHobby : SimpleHobby {}

public class ModifiedSimpleHobby
{
    public int Id { get; set; }
    
    public string ActivityName { get; set; }
}