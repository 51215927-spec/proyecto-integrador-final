# ASP.NET Core y Kestrel

ASP.NET Core es el framework web del servidor. Kestrel recibe las conexiones y entrega las peticiones a la aplicación. `Microsoft.NET.Sdk.Web` aporta la referencia al framework compartido.

1. `WebApplication.CreateBuilder` carga configuración y servicios. `AddSingleton` configura un repositorio y un reloj compartidos durante el proceso.
2. `MapPost`, `MapPut` y `MapGet` enlazan métodos y rutas HTTP a funciones C#. Este estilo se llama Minimal API. Un parámetro `Guid id` viene de la ruta y un objeto `Eleccion` viene del cuerpo JSON.
3. El middleware envuelve la ejecución. Si hay un error de dominio HTTP, JSON inválido o un fallo SQLite reconocido, escribe la respuesta correspondiente.
4. `Results.Ok`, `Results.Created` y `Results.NoContent` indican estado y cuerpo. La lógica de partida se encuentra en el repositorio invocado por el handler.
[Código del servidor](Codigo-del-servidor.md#servidorhttpcs) · [Guía de ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-8.0)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
