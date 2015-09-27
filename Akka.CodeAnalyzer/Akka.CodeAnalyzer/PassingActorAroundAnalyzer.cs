using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Akka.CodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PassingActorAroundAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = nameof(PassingActorAroundAnalyzer);

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }


        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCodeBlockAction(Analyze);
        }

        private static void Analyze(CodeBlockAnalysisContext context)
        {
            if (context.OwningSymbol.Kind != SymbolKind.Method)
            {
                return;
            }

            var method = (IMethodSymbol)context.OwningSymbol;

            foreach (var parameter in method.Parameters)
            {
                if (parameter.Type.AllInterfaces.Select(x => x.MetadataName).Contains("IInternalActor"))
                {
                    var diag = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name,
                        parameter.ContainingSymbol.Name);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}
