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
