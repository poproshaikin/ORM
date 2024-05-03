using ORM_0._3.Core.Enums;

namespace ORM_0._3.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConstraintAttribute : Attribute
{
    public List<DbConstraint> Constraints { get; private set; }

    public ConstraintAttribute(params DbConstraint[] constraints)
    {
        Constraints = constraints.ToList();
    }
}