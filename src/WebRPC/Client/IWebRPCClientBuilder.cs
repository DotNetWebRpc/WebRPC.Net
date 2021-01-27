using System;

namespace WebRPC
{
    public interface IWebRPCClientBuilder
    {
        IWebRPCClientBuilder AddClient<TService, TImplementation>(Action<WebRPCClientSettings<TService>> settings)
           where TService : class
           where TImplementation : class, TService;
        IWebRPCClientBuilder AddClient<TService, TImplementation>()
           where TService : class
           where TImplementation : class, TService;
    }
}
