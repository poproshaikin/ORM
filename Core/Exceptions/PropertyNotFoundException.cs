namespace ORM.Core.Exceptions;

internal class PropertyNotFoundException : Exception
{
    public PropertyNotFoundException(string propertyName, Type type) :
        base($"Property with name {propertyName} wasn't found in {type.FullName}")
    {
    }
}