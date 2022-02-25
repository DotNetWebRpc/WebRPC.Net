using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebRPC
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
            _services.AddScoped<TService, TImplementation>();
            Routes.Add(typeof(TService).FullName, CreateRoute<TService>());
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
                var parameters = new List<ApplicationModel>();
                var id = CreateId(Guid.NewGuid().ToString(), method);

                var methodCache = new MethodCache()
                {
                    Method = method,
                    IsAsync = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null
                };

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
                var methodString = CalculateHash(mi);
                var hash = Hash(methodString);
                Console.WriteLine(hash + "\t" + methodString);

                return hash;
            }
            throw new Exception("Method Information is not a Method: " + m.Name);
        }

        private string CalculateHash(MethodInfo methodInfo)
        {
            Type returnType = methodInfo.ReturnType;
            ParameterInfo[] parameters = methodInfo.GetParameters();

            StringBuilder ret = new StringBuilder();
            ret.Append(methodInfo.Name + ":")
               .Append(GetName(returnType));

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
                else if (IsParams(parameter))
                {
                    ret.Append("|%");
                }
                else
                {
                    ret.Append("|-");
                }

                ret.Append(GetName(parameter.ParameterType));
            }

            return ret.ToString();
        }
        private bool IsParams(ParameterInfo param)
        {
            return param.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }
        private string GetName(Type type)
        {
            if (type == typeof(void))
            {
                return "System.Void";
            }
            else if (type.IsGenericType)
            {
                string subTypes = string.Join(",", type.GenericTypeArguments.Select(t => GetName(t)));
                int num = type.GenericTypeArguments.Length;

                return $"{type.Namespace}.{type.Name.Replace("`" + num, "")}<{subTypes}>";
            }
            //else if (type.IsGenericType && !type.IsInterface)
            //{
            //    string subTypes = string.Join(",", type.GenericTypeArguments.Select(t => GetName(t)));

            //    return $"{GetName(type.BaseType)}<{subTypes}>";
            //}
            else
            {
                return type.FullName.Replace("&", "");
            }
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
