using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;

namespace Surge.Editor.Extensions
{
    internal static class SurgeExtensions
    {
        public static DeclaringPass Run<T>(this Sequence sequence) where T : Pass<T>, new()
        {
            return sequence.Run(new T());
        }
    }
}