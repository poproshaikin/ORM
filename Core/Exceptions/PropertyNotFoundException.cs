namespace ORM.Core.Exceptions;

public class PropertyNotFoundException : Exception
{
    public PropertyNotFoundException(string propertyName, Type type) :
        base($"Property with name {propertyName} wasn't found in {type.FullName}")
    {
    }
}