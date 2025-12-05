using Blueprint.HttpBinder;
using Sample.Enums;

namespace Sample.Classes;

[HttpBinder]
public partial class UserFormCommandClass
{
    public EnumExample? EnumExample { get; set; }
}

[HttpBinder]
public partial record class UserFormCommandRecord
{
    public EnumExample? EnumExample { get; set; }
}

[HttpBinder]
public partial record UserQueryRecord(Guid Id);

[HttpBinder]
public partial class InterfacedClass : IUserFormCommandInterface
{
    public string InitOnlyProperty { get; init; } = null!;
}

public interface IUserFormCommandInterface
{
    string InitOnlyProperty { get; init; }
}

[HttpBinder]
public partial class NestedClassOuter
{
    public NestedClassInner NestedClasses { get; set; } = new();

    public partial class NestedClassInner
    {
        public decimal DecimalProperty { get; set; }
        public decimal DecimalProperty2 { get; set; }

    }
}