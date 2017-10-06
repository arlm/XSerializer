# XSerializer - serialize *anything*


XSerializer is a library that provides advanced, high-performance XML and JSON serializers.

#### XML

XSerializer's XML serialization handles properties and types that the System.Xml.Serialization.XmlSerializer does not handle, such as interfaces and dictionaries. It is meant to be a drop-in replacement for the BCL XmlSerializer - it uses the same attributes: [XmlElement], [XmlAttribute], etc.

```c#
// Create an instance of a serializer.
XmlSerializer<Foo> serializer1 = new XmlSerializer<Foo>();

// If the type of object is not known at compile time, use a factory method.
IXSerializer serializer2 = XmlSerializer.Create(someType);
```

#### JSON

JSON serialization in XSerializer has better performance than JSON.NET (Newtonsoft). It also has better support for deserialization into a variable or field of type dynamic.

```c#
// Create an instance of a serializer.
JsonSerializer<Foo> serializer1 = new JsonSerializer<Foo>();

// If the type of object is not known at compile time, use a factory method.
IXSerializer serializer2 = JsonSerializer.Create(someType);
```

#### Encryption

XSerializer's XML and JSON serializers support the concept of field-level encryption. Properties that are decorated with an [Encrypt] attribute have their values encrypted/decrypted automatically. The actual mechanism to encrypt these fields is exposed through the IEncryptionMechanism interface - users of the encrypt feature are expected to implement this interface in their project.

```c#
public class Foo
{
    // This property is sensitive and should be encrypted.
    [Encrypt]
    public int Bar { get; set; }
    
    // This property is not sensitive and can be serialized in plain text.
    public DateTime Baz { get; set; }
}
```

## Typical Usage

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
