using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.CodeAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.CodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClosingOverActorStateAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = nameof(PassingActorAroundAnalyzer);

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PassingActorAroundAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.PassingActorAroundMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PassingActorAroundDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCodeBlockAction(Analyze);
        }

        private static void Analyze(CodeBlockAnalysisContext context)
        {
            var parent = context.CodeBlock.FirstAncestorOrSelf<ClassDeclarationSyntax>(x => true);
            var shouldcheck = IsFromActor(context.OwningSymbol.ContainingType);
            if (!shouldcheck) return;

      var zz = new Task(() =>
      {
          
      });
           // context.OwningSymbol.ContainingSymbol.
           // var sm = context.SemanticModel.GetDeclaredSymbol(parent);
            var invocations = context.CodeBlock
                .DescendantNodesOfType<InvocationExpressionSyntax>()
                .Where(x =>
                {
                     var access = x.Expression as MemberAccessExpressionSyntax;
                    if (access != null)
                    {
                        var info = context.SemanticModel.GetSymbolInfo(access);
                        if (info.Symbol?.ContainingNamespace.MetadataName == "Tasks")
                            return true;
                    }
                    return false;
                }).ToList();

            var tasks = context.CodeBlock
                .DescendantNodes(_ => true)
                .OfType<ObjectCreationExpressionSyntax>()
                .SelectMany(x => x.DescendantNodes(_ => true).OfType<LambdaExpressionSyntax>());



            var lambdasInside =
                invocations.SelectMany(x => x.DescendantNodesOfType<LambdaExpressionSyntax>())
                    .SelectMany(x => x.DescendantNodesOfType<MemberAccessExpressionSyntax>());

            foreach (var lambda in lambdasInside)
            {
                var access = context.SemanticModel.GetSymbolInfo(lambda.Expression);
                if (IsFromActor(access.Symbol.ContainingType))
                {
                    var diag = Diagnostic.Create(Rule, lambda.GetLocation(), access.Symbol.Name, access.Symbol.Name);
                    context.ReportDiagnostic(diag);
                }
            }
        }

        private static bool IsFromActor(INamedTypeSymbol containingType)
        {
           return containingType
                ?.AllInterfaces
                .Select(x => x.MetadataName)
                .Contains("IInternalActor") == true;
        }
    }
}
