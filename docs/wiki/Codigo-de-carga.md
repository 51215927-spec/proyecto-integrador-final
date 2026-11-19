# Código de la herramienta de alta


## `Program.cs`

[Ver `src/subir-personajes/Program.cs`](https://github.com/51215927-spec/proyecto-integrador-final/blob/4fd69865b1be3ab6b0627eb689191efb7b0a988a/src/subir-personajes/Program.cs)

En el mismo archivo viven el punto de entrada y el diálogo de consola. La herramienta manda solicitudes reales al servidor.

- `Program.Main` valida una URL HTTP o HTTPS opcional, usa `http://localhost:5000` por defecto y lee `Catalogo__Credencial`. Construye un `HttpClient` que mantiene esa dirección base y deshabilita la redirección automática. Delega el formulario a `ConsolaAlta.Ejecutar`.
- La función local `PedirTexto` escribe una etiqueta y devuelve una línea, o señala fin de entrada. `PedirEntero` repite la lectura hasta obtener un entero en el intervalo de esa pregunta.
- `Ejecutar` lee nombre, descripción, arte y puntajes. Serializa los campos con `JsonContent.Create`, añade la credencial Bearer y envía un POST. Traduce los estados de error a mensajes. En un `201` lee `AltaConfirmada` y verifica un ID positivo antes de informar éxito.
- `Ejecutar` pregunta si se desea otra alta y vuelve al principio solo si se confirma. Trata fin de entrada, errores de red, cancelación y JSON inválido. `EsSi` acepta `sí` o `si` sin distinguir mayúsculas.

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
