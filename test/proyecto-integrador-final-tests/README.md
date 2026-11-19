# Pruebas automatizadas

Usa xUnit y referencia los proyectos de servidor, cliente y carga. Verifica reglas del dominio, contratos HTTP y comportamiento de consola. Desde la raíz ejecutá `dotnet test proyecto-integrador-final.sln`.

[Arquitectura](../../docs/arquitectura.md) · [Instalación y uso](../../docs/instalacion-y-uso.md) · [Pruebas](../../docs/plan-de-pruebas.md)

Las pruebas de contrato que arrancan procesos requieren compilar primero la solución y definir `GUERRA_CLIENTE_DLL` y `GUERRA_SERVIDOR_DLL` con las rutas absolutas a `src/cliente/bin/Debug/net8.0/cliente.dll` y `src/servidor/bin/Debug/net8.0/servidor.dll`, respectivamente. Con otra configuración de compilación, ajustá el segmento `Debug`.
