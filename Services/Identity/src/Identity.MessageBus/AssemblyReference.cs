using System.Reflection;

namespace Identity.MessageBus;

public class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}