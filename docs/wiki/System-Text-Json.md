# System.Text.Json

Serializar convierte datos C# en JSON. Deserializar reconstruye datos C# desde ese texto. La API usa ambos pasos para comunicarse entre procesos.

1. En una petición de alta, ASP.NET Core deserializa el cuerpo a `AltaPersonaje` y el método `Validar` aplica las reglas de negocio al objeto resultante.
2. `ServidorHttp.Crear` configura `JsonNumberHandling.Strict`. El puntaje `650` llega como número. El valor `"650"` es texto y se rechaza para un campo entero.
3. `PresentacionVisible` organiza qué campos pueden salir por HTTP. `JsonIgnoreCondition.WhenWritingNull` omite motivo y ganador cuando sus valores son nulos.
4. Varias pruebas usan `JsonDocument` y `JsonElement` para comprobar propiedades concretas del cuerpo, como `revision` o `solicitud`.
[Código de la API](Codigo-del-servidor.md) · [Documentación de System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)

[Recorrido del código](Recorrido-del-codigo.md) · [Índice de documentación](README.md)
