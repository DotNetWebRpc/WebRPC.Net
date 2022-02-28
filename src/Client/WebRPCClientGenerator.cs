using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WebRPC
{
    [Generator]
    public class WebRPCClientGenerator : ISourceGenerator
    {
        private readonly List<string> imports = new List<string>
        {
            "using System;",
            "using System.Collections.Generic;",
            "using System.IO;",
            "using System.Reflection;",
            "using System.Security.Cryptography;",
            "using System.Text;",
            "using System.Threading.Tasks;",
            "using System.Net;",
            "using System.Net.Http;",
            "using MessagePack;",
            "using MessagePack.Resolvers;",
            "using WebRPC;"
        };

        public void Execute(GeneratorExecutionContext context)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();


            if (context.SyntaxReceiver is WebRPCSyntaxReceiver syntaxReciver)
            {
                foreach (var @class in syntaxReciver.Classes)
                {
                    string name = @class.Identifier.Text;
                    var @interface = GetInterface(@class);

                    if (!TryGetParentSyntax(@class, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
                    {
                        throw new Exception($"Namespace not found for {@class.ToFullString()}");
                    }
                    StringBuilder importBuilder = new StringBuilder();
                    foreach(var import in imports)
                    {
                        importBuilder.AppendLine(import);
                    }
                    //importBuilder.AppendLine("using " + namespaceDeclarationSyntax.Name.ToString().Trim() + ";");

                    foreach (var import in namespaceDeclarationSyntax.Parent.DescendantNodes().OfType<UsingDirectiveSyntax>())
                    {
                        if (!imports.Contains(import.NormalizeWhitespace().ToString().Trim()))
                        {
                            importBuilder.AppendLine(import.NormalizeWhitespace().ToString().Trim());
                        }
                    }

                    var type = context.Compilation.GetTypeByMetadataName($"{namespaceDeclarationSyntax.Name.ToString().Trim()}.{@interface}");
                    
                    var source = GetText(assembly, "WebRPC.Templates.class.template")
                                        .Replace("{Imports}", importBuilder.ToString())
                                        .Replace("{ClassName}", name)
                                        .Replace("{Namespace}", namespaceDeclarationSyntax.Name.ToString().Trim())
                                        .Replace("{InterfaceName}", @interface)
                                        .Replace("{Methods}", GetMethods(type));

                    //source = type == null? "Not Found" : type.ToString();

                    //File.WriteAllText($"C:\\JackHenry\\{Guid.NewGuid()}.cs", source);
                    context.AddSource(name + ".g.cs", SourceText.From(source, Encoding.UTF8));
                }
            }
        }

        private string GetInterface(ClassDeclarationSyntax classSyntax)
        {
            foreach (var attributeLists in classSyntax.AttributeLists)
            {
                foreach (var attribute in attributeLists.Attributes)
                {
                    if (attribute.Name.NormalizeWhitespace().ToString() == "WebRPCClient")
                    {
                        var argument = attribute.ArgumentList.Arguments[0];

                        return argument.Expression.ToString().Replace("typeof(", "").Replace(")", "");
                    }
                }
            }
            return "Not Found";
        }
       
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new WebRPCSyntaxReceiver());
        }

        public bool TryGetParentSyntax<T>(SyntaxNode syntaxNode, out T result)
            where T : SyntaxNode
        {
            // set defaults
            result = null;

            if (syntaxNode == null)
            {
                return false;
            }

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return false;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    result = syntaxNode as T;
                    return true;
                }

                return TryGetParentSyntax<T>(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }

        private bool IsByRef(IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.RefKind == RefKind.Ref;
        }

        private bool IsOut(IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.RefKind == RefKind.Out;
        }

        private string GetRefKind(IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.RefKind != RefKind.None ? parameterSymbol.RefKind.ToString().ToLower() + " " : "";
        }

        private string GetMethods(INamedTypeSymbol type)
        {
            StringBuilder ret = new StringBuilder();

            foreach (var member in type.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol)
                {
                    StringBuilder parameters = new StringBuilder();
                    string methodName = methodSymbol.Name;
                    var returnType = methodSymbol.ReturnType.ToString().Trim();
                    string methodString = CalculateHash(methodSymbol);
                    string id = Hash(methodString);


                    var isAsync = returnType.Contains("Task<");
                    ret.AppendLine("        //" + methodString);

                    parameters.Append(string.Join(", ", methodSymbol.Parameters.Select(p => $"{GetRefKind(p)}{p.Type.ToString().Trim()} {p.Name}")));

                    string async_method = isAsync ? "async " : "";

                    ret.AppendLine($"        public {async_method}{returnType} {methodName}({parameters})")
                       .AppendLine($"        {{");

                    StringBuilder objectString = new StringBuilder();
                    objectString.Append(string.Join(", ", methodSymbol.Parameters.Select(p => IsOut(p) ? "null" : p.Name)));

                    var hasParams = objectString.Length > 0 ? "," : "";

                    string noserializer = "";
                    if (!methodSymbol.Parameters.Any(p => !IsOut(p)))
                    {
                        noserializer = "                await Task.CompletedTask;";
                    }


                    string retText = null;
                    ret.Append("            ").AppendLine(string.Join("\r\n            ", methodSymbol.Parameters.Where(p => !IsOut(p)).Select(p => $"var __local_refs__{p.Name} = {p.Name};")))
                       .AppendLine("            Func<Stream, Task> serializer = async(stream) =>")
                       .AppendLine("            {")
                       .Append("                ").AppendLine(string.Join("\r\n                ", methodSymbol.Parameters.Where(p => !IsOut(p)).Select(p => $"await MessagePackSerializer.SerializeAsync(typeof({p.Type.ToString()}), stream, __local_refs__{p.Name}, ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4BlockArray));")))
                       .AppendLine(noserializer)
                       .AppendLine("            };");

                    if (isAsync)
                    {
                        ret.AppendLine($"            using(var ____response = await MakeRequestAsync(\"{id}\", \"{methodName}\", serializer))")
                           .AppendLine($"            {{");

                        if (returnType != "void")
                        {
                            ret.AppendLine($"                var ____ret = await ____response.ReadAsync<{returnType.Remove(returnType.Length - 1).Replace("System.Threading.Tasks.Task<", "")}>();");
                            retText = $"                return ____ret;";
                        }
                    }
                    else
                    {
                        ret.AppendLine($"            using(var ____response = MakeRequest(\"{id}\", \"{methodName}\", serializer))")
                           .AppendLine($"            {{");

                        if (returnType != "void")
                        {
                            ret.AppendLine($"                var ____ret = ____response.Read<{returnType}>();");
                            retText = $"                return ____ret;";
                        }
                    }

                    ret.Append(string.Join("\r\n", methodSymbol.Parameters.Where(p => IsOut(p) || IsByRef(p)).Select(p => $"                {p.Name} = ____response.Read<{p.Type.ToString()}>();")))
                       .AppendLine();

                    if (retText != null)
                    {
                        ret.AppendLine(retText);
                    }
                    ret.AppendLine($"            }}")
                       .AppendLine($"        }}");
                }
            }
            return ret.ToString();
        }
        private string GetText(Assembly assembly, string name)
        {
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(name)))
            {
                return reader.ReadToEnd();
            }
        }

        private string Hash(string s)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(s)));
            }
        }

        private string CalculateHash(IMethodSymbol methodSymbol)
        {
            StringBuilder ret = new StringBuilder();
            ret.Append(methodSymbol.Name + ":");

            ret.Append(GetSymbolType(methodSymbol.ReturnType));

            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.RefKind == RefKind.Out)
                {
                    ret.Append("|^");
                }
                else if (parameter.RefKind == RefKind.Ref)
                {
                    ret.Append("|*");
                }
                else if (parameter.IsParams)
                {
                    ret.Append("|%");
                }
                else
                {
                    ret.Append("|-");
                }

                ret.Append(GetSymbolType(parameter.Type));
            }

            return ret.ToString();
        }
        public string GetSymbolType(ITypeSymbol symbol)
        {
            StringBuilder ret = new StringBuilder();

            if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
            {
                ret.Append(GetTypeSymbolName(namedTypeSymbol))
                   .Append("<")
                   .Append(GetGenericParameterType(namedTypeSymbol))
                   .Append(">");
            }
            else if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                ret.Append(GetTypeSymbolName(arrayTypeSymbol.ElementType))
                   .Append("[]");

            }
            else
                    {
                ret.Append(GetTypeSymbolName(symbol));
            }

            return ret.ToString();
        }
        public string GetGenericParameterType(INamedTypeSymbol symbol)
        {
            return string.Join(", ", symbol.TypeArguments.Select(p => GetSymbolType(p)));
        }
        public string GetTypeSymbolName(ITypeSymbol symbol)
        {
            return $"{symbol.ContainingNamespace}.{symbol.Name.Trim()}";
        }
    }
}
