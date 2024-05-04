using ORM.Core.Enums;

namespace ORM.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConstraintAttribute : Attribute
{
    public List<DbConstraint> Constraints { get; private set; }

    public ConstraintAttribute(params DbConstraint[] constraints)
    {
        Constraints = constraints.ToList();
    }
}