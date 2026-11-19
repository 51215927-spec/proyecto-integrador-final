# HttpClient y System.Net.Http.Json

`HttpClient` envía mensajes HTTP desde .NET. `System.Net.Http.Json` ofrece ayudas para transformar los cuerpos JSON.

1. `Program.Main` configura la dirección base. `ConsolaAlta.Ejecutar` crea un `HttpRequestMessage` con método POST, ruta y cabecera Bearer.
2. `JsonContent.Create` serializa los datos del personaje en el cuerpo. `SendAsync` envía la petición y devuelve el estado y contenido.
3. La herramienta comprueba el estado 201 y usa `ReadFromJsonAsync<AltaConfirmada>` para leer y comprobar el ID. Una excepción de red o de JSON significa que la herramienta no pudo confirmar el alta.
4. Las pruebas HTTP usan las mismas clases para enviar elecciones y consultar vistas. La integración HTTP del cliente que juega continúa pendiente.
[Código de alta](Codigo-de-carga.md) · [Código de pruebas](Codigo-de-las-pruebas.md)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
