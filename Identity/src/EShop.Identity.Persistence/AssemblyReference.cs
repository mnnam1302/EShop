using System.Reflection;

namespace EShop.Identity.Persistence;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(Assembly).Assembly;
}