using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Inicialización si es necesaria
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Generar código de ejemplo
            var sourceCode = @"
using System;

namespace Generated
{
    public static class GeneratedCode
    {
        public static void Execute()
        {
            Console.WriteLine(""¡Código generado con Roslyn!"");
        }
        
        public static string GetTimestamp()
        {
            return ""Generado el: " + System.DateTime.Now.ToString() + @""";
        }
    }
}";

            // Agregar el código generado a la compilación
            context.AddSource("GeneratedCode.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }
}