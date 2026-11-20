# Pruebas automatizadas

Usa xUnit y referencia los proyectos de servidor, cliente y carga. Verifica reglas del dominio, contratos HTTP y comportamiento de consola. Desde la raíz ejecutá `dotnet test proyecto-integrador-final.sln`.

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)

Las pruebas de contrato que arrancan procesos requieren compilar primero la solución y definir `GUERRA_CLIENTE_DLL`, `GUERRA_SERVIDOR_DLL` y `GUERRA_CARGA_DLL` con las rutas absolutas a `src/cliente/bin/Debug/net8.0/cliente.dll`, `src/servidor/bin/Debug/net8.0/servidor.dll` y `src/subir-personajes/bin/Debug/net8.0/subir-personajes.dll`, respectivamente. Con otra configuración de compilación, ajustá el segmento `Debug`.

Desde PowerShell, después de compilar:

```powershell
$env:GUERRA_CLIENTE_DLL = (Resolve-Path src/cliente/bin/Debug/net8.0/cliente.dll).Path
$env:GUERRA_SERVIDOR_DLL = (Resolve-Path src/servidor/bin/Debug/net8.0/servidor.dll).Path
$env:GUERRA_CARGA_DLL = (Resolve-Path src/subir-personajes/bin/Debug/net8.0/subir-personajes.dll).Path
dotnet test proyecto-integrador-final.sln
```

Estas variables son del arnés de pruebas; los programas se operan mediante preguntas y no necesitan variables de usuario. Las pruebas de procesos envían las respuestas por stdin. [Resultados del spec 003](../../docs/plan-de-pruebas.md#evidencia-del-spec-003)
