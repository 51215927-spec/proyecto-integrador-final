using System;
using Xunit;

public sealed class ContratoFactAttribute : FactAttribute
{
    public ContratoFactAttribute(string variableDll)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableDll)))
            Skip = $"Indicá la DLL compilada mediante {variableDll} para ejecutar esta prueba.";
    }
}
