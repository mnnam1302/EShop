using System.Reflection;

namespace Identity.Persistence;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}