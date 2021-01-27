using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebRPC.Host
{
    public class WebRPCHostBuilder
    {
        private readonly IServiceCollection _services;
        public IRoutes Routes { get; set; }
        public WebRPCHostBuilder(IServiceCollection services)
        {
            _services = services;
            Routes = new Routes();
        }
        public void AddInterface<TService, TImplementation>() where TService : class
            where TImplementation : class, TService
        {
            string route = typeof(TService).FullName;
            _services.AddScoped<TService, TImplementation>();
            Routes.Add(route, CreateRoute<TService>());
        }

        private ApplicationRoute CreateRoute<T>()
        {
            var route = new ApplicationRoute()
            {
                InterfaceType = typeof(T)
            };
            var methodCaches = new Dictionary<string, MethodCache>();
            foreach (var method in route.InterfaceType.GetMethods())
            {
                var id = CreateId(Guid.NewGuid().ToString(), method);
                var methodCache = new MethodCache()
                {
                    Method = method,
                    IsAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null
                };
                List<ApplicationModel> parameters = new List<ApplicationModel>();

                foreach (var parameter in method.GetParameters())
                {
                    parameters.Add(new ApplicationModel()
                    {
                        Name = parameter.Name,
                        Type = parameter.ParameterType,
                        IsOut = parameter.IsOut,
                        IsByRef = parameter.ParameterType.IsByRef
                    });
                }
                methodCache.Parameters = parameters.ToArray();
                methodCaches.Add(id, methodCache);
            }
            route.Methods = new ReadOnlyDictionary<string, MethodCache>(methodCaches);

            return route;
        }
        public string CreateId(string key, MethodBase m)
        {
            if (m == null)
            {
                throw new ArgumentException("MethodBase m is required");
            }
            if (key == null)
            {
                throw new ArgumentException("Key is required");
            }

            if (m is MethodInfo mi)
            {
                return Hash(CalculateHash(mi));
            }
            throw new Exception("Method Information is not a Method: " + m.Name);
        }
        private string CalculateHash(MethodInfo methodInfo)
        {
            Type returnType = methodInfo.ReturnType;
            ParameterInfo[] parameters = methodInfo.GetParameters();

            StringBuilder ret = new StringBuilder();
            ret.Append(methodInfo.Name + ":");

            if (returnType == typeof(void))
            {
                ret.Append("void");
            }
            else
            {
                ret.Append(returnType.FullName);
            }

            foreach (var parameter in parameters)
            {
                if (parameter.IsOut)
                {
                    ret.Append("|^");
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    ret.Append("|*");
                }
                else
                {
                    ret.Append("|-");
                }

                ret.Append(parameter.ParameterType.FullName);
            }

            return ret.ToString();
        }
        private string Hash(string text)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(text)));
            }
        }
    }
}
