namespace stORM.DataAnotattions;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignkeyFrom : Attribute
{
    public string EntityName { get; }

    public ForeignkeyFrom(string entityName)
    {
        EntityName = entityName;
    }
}