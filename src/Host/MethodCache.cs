using System.Reflection;

namespace WebRPC
{
    public class MethodCache
    {
        public bool IsAsync { get; set; }
        public MethodInfo Method { get; set; }
        public ApplicationModel[] Parameters { get; set; }
    }
}
