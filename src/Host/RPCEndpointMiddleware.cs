using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace WebRPC
{
    public class RPCEndpointMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly IRoutes _routes;

        public RPCEndpointMiddleware(ILogger<RPCEndpointMiddleware> logger, RequestDelegate next, IRoutes routes)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _routes = routes;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);

            try
            {
                if (httpContext.Request.Headers.TryGetValue("X-MethodSignature", out StringValues values1) && httpContext.Request.Headers.TryGetValue("X-MethodHandler", out StringValues values2))
                {
                    if (_routes.TryGetValue(values2[0], out ApplicationRoute route) && route.Methods.TryGetValue(values1[0], out MethodCache methodCache))
                    {
                        var o = httpContext.RequestServices.GetService(route.InterfaceType);

                        if (o != null)
                        {
                            var parameters = await GetParameters(httpContext.Request, methodCache.Parameters);

                            object result;
                            if (methodCache.IsAsync)
                            {
                                dynamic awaitable = methodCache.Method.Invoke(o, parameters);
                                await awaitable;
                                result = awaitable.GetAwaiter().GetResult();
                            }
                            else
                            {
                                result = methodCache.Method.Invoke(o, parameters);
                            }
                            httpContext.Response.StatusCode = 200;

                            await WriteResponseAsync(methodCache, httpContext.Response.Body, result);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                await MessagePackSerializer.SerializeAsync(httpContext.Response.Body, ex, MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
                return;
            }

            httpContext.Response.StatusCode = 404;
        }

        private async Task<object[]> GetParameters(HttpRequest request, ApplicationModel[] parameters)
        {
            var ret = new object[parameters.Length];
            using (var reader = new MessagePackStreamReader(request.Body))
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].IsOut && await reader.ReadAsync(new System.Threading.CancellationToken()) is ReadOnlySequence<byte> msgpack)
                    {
                        ret[i] = MessagePackSerializer.Deserialize(parameters[i].Type, msgpack, MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
                    }
                }
            }
            return ret;
        }

        private async Task WriteResponseAsync(MethodCache methodCache, Stream body, object result)
        {
            await MessagePackSerializer.SerializeAsync(body, result, MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));

            for (int i = 0; i < methodCache.Parameters.Length; i++)
            {
                if (methodCache.Parameters[i].IsByRef || methodCache.Parameters[i].IsOut)
                {
                    await MessagePackSerializer.SerializeAsync(body, result, MessagePack.Resolvers.ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));
                }
            }
        }
    }
}
