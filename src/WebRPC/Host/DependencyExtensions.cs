using Microsoft.AspNetCore.Builder;
using WebRPC.Host;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyExtensions
    {
        public static IApplicationBuilder UseWebRPCHost(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<RPCEndpointMiddleware>();
        }
       
        public static IServiceCollection AddWebRPCHost(this IServiceCollection services, Action<WebRPCHostBuilder> build)
        {
            var builder = new WebRPCHostBuilder(services);

            build(builder);

            services.AddSingleton<IRoutes>(builder.Routes);

            return services;
        }
    }
}
