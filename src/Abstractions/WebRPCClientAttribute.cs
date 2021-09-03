using System;

namespace WebRPC
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class WebRPCClientAttribute : Attribute
    {
        public static string Name = typeof(WebRPCClientAttribute).FullName.Replace("Attribute", string.Empty);
        public Type InterfaceType { get; set; }
        public WebRPCClientAttribute(Type type)
        {
            InterfaceType = type;
        }
    }
}
