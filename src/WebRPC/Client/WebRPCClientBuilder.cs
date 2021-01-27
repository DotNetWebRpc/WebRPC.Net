using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRPC
{
    public class WebRPCClientBuilder : IWebRPCClientBuilder
    {
        private readonly IServiceCollection _services;

        public WebRPCClientBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IWebRPCClientBuilder AddClient<TService, TImplementation>(Action<WebRPCClientSettings<TService>> settings)
            where TService : class
            where TImplementation : class, TService
        {
            var setting = new WebRPCClientSettings<TService>();
            settings(setting);

            _services.AddSingleton<WebRPCClientSettings<TService>>(setting);
            AddCommon<TService, TImplementation>();
            return this;
        }

        public IWebRPCClientBuilder AddClient<TService, TImplementation>()
           where TService : class
           where TImplementation : class, TService
        {
            _services.AddSingleton<WebRPCClientSettings<TService>>(new WebRPCClientSettings<TService>());
            AddCommon<TService, TImplementation>();
            return this;
        }

        private void AddCommon<TService, TImplementation>()
             where TService : class
            where TImplementation : class, TService
        {
            _services.AddHttpClient<Client<TService>>();
            _services.AddTransient<TService, TImplementation>();
        }
    }
}
