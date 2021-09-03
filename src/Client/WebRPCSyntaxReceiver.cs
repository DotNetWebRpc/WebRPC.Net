using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace WebRPC
{
    internal sealed class WebRPCSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax && HaveAttribute(classSyntax, "WebRPCClient"))
            {
                Classes.Add(classSyntax);
            }
        }

        private bool HaveAttribute(ClassDeclarationSyntax classSyntax, string attributeName)
        {
            foreach (var attributeLists in classSyntax.AttributeLists)
            {
                foreach (var attribute in attributeLists.Attributes)
                {
                    if (attribute.Name.NormalizeWhitespace().ToString() == attributeName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
