# Archivos de compilación


## `proyecto-integrador-final.sln`

[Ver `proyecto-integrador-final.sln`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/proyecto-integrador-final.sln)

La solución reúne los tres ejecutables y el proyecto de pruebas. Relaciona las configuraciones Debug y Release de la solución con los proyectos.

## `cliente.csproj`

[Ver `src/cliente/cliente.csproj`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/cliente/cliente.csproj)

Usa el SDK de .NET para generar un ejecutable `net8.0`. Declara Spectre.Console y permite al ensamblado de pruebas acceder a tipos internos.

## `servidor.csproj`

[Ver `src/servidor/servidor.csproj`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/servidor/servidor.csproj)

Usa el SDK Web para incluir ASP.NET Core. Declara el proveedor SQLite de EF Core y habilita acceso a tipos internos desde las pruebas.

## `subir-personajes.csproj`

[Ver `src/subir-personajes/subir-personajes.csproj`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/subir-personajes/subir-personajes.csproj)

Declara el ejecutable `net8.0` de alta. Usa las clases HTTP y JSON de .NET y permite probar clases internas.

## `proyecto-integrador-final-tests.csproj`

[Ver `test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/test/proyecto-integrador-final-tests/proyecto-integrador-final-tests.csproj)

Referencia los tres ejecutables. Declara xUnit, su adaptador, el SDK de pruebas y Coverlet. Unifica el nombre ensamblado que los proyectos usan en `InternalsVisibleTo`.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
