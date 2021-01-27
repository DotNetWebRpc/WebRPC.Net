using System;

namespace WebRPC
{
    public class WebRPCClientSettings<I> where I : class
    {
        public string Url { get; set; }
        public int Timeout { get; set; } = 60000;
    }
}
