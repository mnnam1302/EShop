using System.Reflection;

namespace EShop.Shared.Scoping;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}