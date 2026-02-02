using Blueprint.HttpBinder.Sample.Enums;

namespace Blueprint.HttpBinder.Sample.Classes;

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
    public List<NestedClassInner> NestedClassList { get; set; } = [];

    public partial class NestedClassInner
    {
        public decimal DecimalProperty { get; set; }
        public decimal DecimalProperty2 { get; set; }
        public decimal InitOnlyProperty { get; init; }
    }
}

[HttpBinder]
public partial class HttpBinderClassWithHttpBinderBase : HttpBinderClassBase
{
    public decimal DecimalProperty { get; set; }
}

[HttpBinder]
public partial class HttpBinderClassBase;

[HttpBinder]
public partial class ClassWithBindAsyncBase : BindAsyncClassBase;

public class  BindAsyncClassBase
{
    public static void BindAsync(HttpContext httpContext)
    {}
}