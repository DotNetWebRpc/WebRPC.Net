using WebRPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientBuilderExtensions
    {

        public static WebRPCClientBuilder AddWebRPC(this IServiceCollection services)
        {
            return new WebRPCClientBuilder(services);
        }
    }
}
