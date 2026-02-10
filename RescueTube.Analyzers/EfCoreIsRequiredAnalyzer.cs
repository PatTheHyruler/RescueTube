using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace RescueTube.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EfCoreIsRequiredAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor RequiredNullableProperty = new(
        id: "RTEF0001",
        title: "Nullable property should not be configured as IsRequired(true)",
        messageFormat: "Property '{0}' is nullable, but is configured using IsRequired(true)",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NotRequiredNonNullableProperty = new(
        id: "RTEF0002",
        title: "Non-nullable property should not be configured as IsRequired(false)",
        messageFormat: "Property '{0}' is not nullable, but is configured using IsRequired(false)",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        RequiredNullableProperty,
        NotRequiredNonNullableProperty,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeMethodInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeMethodInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocationOperation ||
            invocationOperation.TargetMethod.Name != "IsRequired" ||
            invocationOperation.TargetMethod.ContainingNamespace.ToString() !=
            "Microsoft.EntityFrameworkCore.Metadata.Builders")
        {
            return;
        }

        IInvocationOperation? propertySelectorMethodInvocationOperation = null;
        var currentOperation = invocationOperation;
        var depthLeft = 50;
        while (depthLeft > 0)
        {
            depthLeft--;
            var instance = currentOperation.Instance;
            if (instance is not IInvocationOperation previousInvocationOperation)
            {
                break;
            }

            if (previousInvocationOperation.TargetMethod.Name is "Property" or "ComplexProperty" or "HasForeignKey" or "HasOne")
            {
                propertySelectorMethodInvocationOperation = previousInvocationOperation;
                break;
            }

            currentOperation = previousInvocationOperation;
        }

        if (propertySelectorMethodInvocationOperation is null)
        {
            return;
        }

        var propertySelectorArgumentOperation = propertySelectorMethodInvocationOperation.Arguments.FirstOrDefault()?.Value;
        if (propertySelectorArgumentOperation.DescendantsAndSelf().FirstOrDefault(x => x is IPropertyReferenceOperation)
            is not IPropertyReferenceOperation propertyReferenceOperation)
        {
            return;
        }

        bool? isNullable = propertyReferenceOperation.Type switch
        {
            { IsValueType: true, Name: "Nullable", ContainingNamespace.Name: "System" } => true,
            _ => null,
        };

        isNullable ??= propertyReferenceOperation.Type?.NullableAnnotation switch
        {
            NullableAnnotation.Annotated => true,
            NullableAnnotation.NotAnnotated => false,
            NullableAnnotation.None or _ => null,
        };

        var argument = invocationOperation.Arguments.FirstOrDefault();

        bool? isRequired;
        if (argument?.Value is ILiteralOperation { ConstantValue: { HasValue: true, Value: bool value } })
        {
            isRequired = value;
        }
        else
        {
            return;
        }

        var propertyName = propertyReferenceOperation.Property.Name;

        if (isRequired is true && isNullable is true)
        {
            var operationLocation = invocationOperation.Syntax.GetLocation();
            var diagnostic = Diagnostic.Create(RequiredNullableProperty, operationLocation, propertyName);
            context.ReportDiagnostic(diagnostic);
        }
        else if (isRequired is false && isNullable is false)
        {
            var operationLocation = invocationOperation.Syntax.GetLocation();
            var diagnostic = Diagnostic.Create(NotRequiredNonNullableProperty, operationLocation, propertyName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}