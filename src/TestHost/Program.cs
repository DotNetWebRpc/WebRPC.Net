using Microsoft.Extensions.DependencyInjection;
using System;
using WebRPC.Models;

namespace TestHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new WebRPC.WebRPCHostBuilder(new ServiceCollection());
            builder.AddInterface<INullableService, NullableService>();
            builder.AddInterface<ISimpleService, SimpleService>();
            builder.AddInterface<IComplexService, ComplexService>();

        }
    }
}
