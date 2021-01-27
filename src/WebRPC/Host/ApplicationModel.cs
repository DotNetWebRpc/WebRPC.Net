using System;

namespace WebRPC.Host
{
    public class ApplicationModel
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsOut { get; set; }
        public bool IsByRef { get; set; }
    }
}
