# Carga de personajes desde JSON

Con el servidor iniciado y una credencial de catálogo configurada, ejecutá desde la raíz:

```sh
dotnet run --project src/subir-personajes -- "src/subir-personajes/ejemplos/personajes.json"
```

El argumento es la ruta del JSON. Si se omite, se pregunta en consola. Las rutas relativas parten del directorio de ejecución; usá comillas para rutas con espacios. Después se solicitan URL (Enter: `http://localhost:5000`) y credencial de catálogo, que no se muestra.

La herramienta muestra la cantidad actual y la del archivo antes de pedir confirmación. Solo `sí` o `si` confirma, ignorando mayúsculas y espacios externos. `no` o fin de entrada cancela; otra respuesta repite únicamente la confirmación. **El archivo reemplaza todo el catálogo**, incluidos personajes inactivos. Repetirlo no acumula copias. Prepará el catálogo fuera de las partidas.

El JSON siempre es una lista, incluso para un solo personaje:

```json
[
  {
    "nombre": "Herder",
    "descripcion": "Guerrero protector del bosque.",
    "ascii": "  O\n /|\\\n / \\",
    "fuerza": 700,
    "velocidad": 450,
    "resistencia": 600,
    "inteligencia": 400,
    "energia": 500,
    "habilidad": 350
  }
]
```

Todos los campos son obligatorios: tres textos y seis enteros. No incluyas ID; el servidor asigna 1..N en orden y activa todos los personajes. La lista no puede estar vacía ni repetir nombres ignorando mayúsculas y espacios externos. Nombre no puede estar vacío; descripción y ASCII pueden ser cadenas vacías. ASCII no admite tabulaciones y permite hasta 25 líneas de 120 caracteres, contando LF, CRLF o CR. Cada atributo debe estar entre 250 y 750; los seis suman 3000, con al menos uno de 650 o más y uno de 350 o menos.

Los seis ejemplos reutilizan nombres y atributos de la demostración del cliente. Se cargan por el mismo flujo, indicando su ruta; nunca se cargan automáticamente.

Los errores de archivo, formato, validación o guardado finalizan con código 1, sin reintentos. El servidor valida el lote completo y guarda en una sola transacción: un rechazo o fallo de guardado conserva el catálogo anterior. Si se pierde la respuesta después de enviar el reemplazo, el resultado es desconocido: el catálogo puede ser el anterior o el nuevo completo. La herramienta informa esa incertidumbre y no afirma un rollback. Éxito o cancelación voluntaria terminan con código 0. Para recuperar un catálogo reemplazado, cargá su JSON anterior.

[Instalación y uso](../../docs/instalacion-y-uso.md) · [Protocolo HTTP](../../docs/protocolo-de-partida.md)
