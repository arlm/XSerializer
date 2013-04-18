#XSerializer - serialize *anything*#

##Typical Usage##

You can serialize/deserialize like this:

```C#
public class Foo
{
    public int Id { get; set; }
    public IBar Bar { get; set; }
}

[XmlInclude(typeof(Bar))] // Method #1 uses attributes to indicate the inheritors of the interface.
public interface IBar
{
    int Id { get; set; }
    string Value { get; set; }
}

public class Bar : IBar
{
    public int Id { get; set; }
    public string Value { get; set; }
}

public void Main()
{
    Foo foo = new Foo { Id = 1, Bar = new Bar { Id = 2, Value = "Baz" } };
    
    XmlSerializer<Foo> serializer = new XmlSerializer<Foo>();
    
    string xml = serializer.Serialize(foo);
    Foo roundTripFoo = serializer.Deserialize(xml);
}
```

Or this:

```C#
public class Foo
{
    public int Id { get; set; }
    public IBar Bar { get; set; }
}

public interface IBar
{
    int Id { get; set; }
    string Value { get; set; }
}

public class Bar : IBar
{
    public int Id { get; set; }
    public string Value { get; set; }
}

public void Main()
{
    Foo foo = new Foo { Id = 1, Bar = new Bar { Id = 2, Value = "Baz" } };
    
    // Method #2 passes in the types of the inheritors to the constructor.
    XmlSerializer<Foo> serializer = new XmlSerializer<Foo>(typeof(Bar));
    
    string xml = serializer.Serialize(foo);
    Foo roundTripFoo = serializer.Deserialize(xml);
}
```
