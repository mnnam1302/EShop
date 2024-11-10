using System.Reflection;

namespace EShop.Shared.Contracts;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}