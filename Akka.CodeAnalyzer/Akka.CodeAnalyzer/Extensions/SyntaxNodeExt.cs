using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Akka.CodeAnalyzer.Extensions
{
    public static class SyntaxNodeExt
    {
        public static IEnumerable<TType> DescendantNodesOfType<TType>(this SyntaxNode node)
        {
            return node.DescendantNodes(_ => true).OfType<TType>();
        }
    }
}
