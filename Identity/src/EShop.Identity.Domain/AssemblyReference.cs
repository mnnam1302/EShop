using System.Reflection;

namespace EShop.Identity.Domain;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}