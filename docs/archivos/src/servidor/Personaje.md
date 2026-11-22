# Personaje.cs

Fuente: [src/servidor/Personaje.cs](../../../../src/servidor/Personaje.cs). Proyecto: `src/servidor/servidor.csproj` (`net8.0`). Namespace: `Proyecto_Integrador_Final`. Versión común: [catálogo](../../README.md#versión-documentada).

## Propósito y responsabilidad

Representa una carta del modelo local con nombre, arte y seis puntajes. Construirla verifica las reglas numéricas; los getters permiten que Combate compare un atributo. No representa por sí misma el ID de catálogo, la descripción ni el indicador activo de la API.

## Dependencias

Utiliza `System.ArgumentException` y `ArgumentOutOfRangeException`. No requiere servicios, archivos ni configuración. Lo consumen el modelo de combate, las pruebas y la validación del catálogo. Véanse [reglas del juego](../../../reglas-del-juego.md) y [persistencia](../../../bases-de-datos.md) para las restricciones adicionales del catálogo.

## Estructura

`internal enum TipoAtributo` declara, en este orden, `Fuerza = 0`, `Velocidad = 1`, `Resistencia = 2`, `Inteligencia = 3`, `Energia = 4`, `Habilidad = 5` por numeración implícita de C#. No tiene flags ni métodos propios.

`internal class Personaje` no tiene base explícita, interfaces, atributos ni partial. No declara propiedades, eventos, operadores ni funciones locales. Los métodos públicos siguen siendo accesibles solo donde lo permita el tipo interno y la amistad con el ensamblado de pruebas.

## Estado y datos

Ocho campos privados: `string nombre`, `string ascii` y los enteros `fuerza`, `velocidad`, `resistencia`, `inteligencia`, `energia`, `habilidad`. El constructor los asigna; ningún método posterior los modifica, aunque no se declaran readonly. Una instancia construida conserva su estado durante toda su vida. No se recorta ni normaliza texto. El arte vacío se permite; no se comprueban aquí longitud, líneas ni controles de terminal.

## Miembros y ejecución

### `public Personaje(string nombre, string ascii, int fuerza, int velocidad, int resistencia, int inteligencia, int energia, int habilidad)`

Rechaza primero nombre null, vacío o solo espacios mediante `ArgumentException` con parámetro `nombre`. Después rechaza ASCII null mediante `ArgumentException` con parámetro `ascii`. Copia los ocho argumentos y llama a `Validar`. Una excepción de validación impide devolver una instancia construida. No hay efectos persistidos ni cambios sobre otros objetos.

### `private void Validar()`

Calcula la suma entera de los seis campos. Recorre un array de seis tuplas `(campo, valor)` en el orden fuerza, velocidad, resistencia, inteligencia, energia, habilidad. Para el primer valor menor a 250 o mayor a 750 lanza `ArgumentException`, con el nombre de ese campo. Si todos están dentro del rango, comprueba en orden: suma exactamente 3000, existencia de un valor ≥650, existencia de uno ≤350. Cada incumplimiento lanza `ArgumentException` con parámetro `atributos`. El primer error interrumpe la validación; no devuelve una lista de errores ni corrige valores.

Fragmento real de la comprobación de suma y fortaleza; el bucle de rangos lo precede y la comprobación de debilidad lo sigue:

```csharp
            if (suma != 3000)
            {
                throw new ArgumentException(
                    "La suma de los atributos debe ser exactamente 3000.", "atributos");
            }

            if (fuerza < 650 && velocidad < 650 &&
                resistencia < 650 && inteligencia < 650 &&
                energia < 650 && habilidad < 650)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser mayor o igual a 650.", "atributos");
            }
```

La conjunción exige que fallen los seis valores para rechazar por falta de fortaleza. Basta uno igual a 650 para superar esa comprobación. La condición de debilidad es simétrica: rechaza si los seis son mayores que 350; acepta un 350 exacto.

### Getters

`public string GetNombre()` y `public string GetAscii()` retornan respectivamente nombre y arte sin transformar ni mutar. `public int GetAtributo(TipoAtributo tipo)` devuelve el campo seleccionado. No itera ni consulta una base de datos:

```csharp
            return tipo switch
            {
                TipoAtributo.Fuerza => fuerza,
                TipoAtributo.Velocidad => velocidad,
                TipoAtributo.Resistencia => resistencia,
                TipoAtributo.Inteligencia => inteligencia,
                TipoAtributo.Energia => energia,
                TipoAtributo.Habilidad => habilidad,

                _ => throw new ArgumentOutOfRangeException(nameof(tipo))
            };
```

El caso `_` cubre enteros convertidos al enum sin una constante declarada: `(TipoAtributo)6` lanza, en vez de devolver cero. El `switch` es una expresión cuyo resultado se retorna directamente.

### Fragmentos complementarios por miembro

Los bloques siguientes corresponden a los miembros explicados arriba; se omiten los demás miembros del archivo, sin omitir condiciones internas de estos bloques.

Constructor: las guardas preceden a las asignaciones y a Validar.

```csharp
        public Personaje(
            string nombre,
            string ascii,
            int fuerza,
            int velocidad,
            int resistencia,
            int inteligencia,
            int energia,
            int habilidad)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            if (ascii == null)
                throw new ArgumentException("El ASCII no puede ser nulo.", nameof(ascii));

            this.nombre = nombre;
            this.ascii = ascii;

            this.fuerza = fuerza;
            this.velocidad = velocidad;
            this.resistencia = resistencia;
            this.inteligencia = inteligencia;
            this.energia = energia;
            this.habilidad = habilidad;

            Validar();
        }
```

Validar: recorrido de rangos y condiciones completas, incluidos umbrales inclusivos.

```csharp
        private void Validar()
        {
            // El total fija el presupuesto de puntos y los umbrales exigen una fortaleza y una debilidad.
            int suma = fuerza + velocidad + resistencia
                     + inteligencia + energia + habilidad;

            foreach (var (campo, valor) in new[]
            {
                (nameof(fuerza), fuerza), (nameof(velocidad), velocidad),
                (nameof(resistencia), resistencia), (nameof(inteligencia), inteligencia),
                (nameof(energia), energia), (nameof(habilidad), habilidad)
            })
            {
                if (valor < 250 || valor > 750)
                    throw new ArgumentException("El atributo debe estar entre 250 y 750.", campo);
            }

            if (suma != 3000)
            {
                throw new ArgumentException(
                    "La suma de los atributos debe ser exactamente 3000.", "atributos");
            }

            if (fuerza < 650 && velocidad < 650 &&
                resistencia < 650 && inteligencia < 650 &&
                energia < 650 && habilidad < 650)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser mayor o igual a 650.", "atributos");
            }

            if (fuerza > 350 && velocidad > 350 &&
                resistencia > 350 && inteligencia > 350 &&
                energia > 350 && habilidad > 350)
            {
                throw new ArgumentException(
                    "Al menos un atributo debe ser menor o igual a 350.", "atributos");
            }
        }
```

## Recorridos y ejemplos

Normal: nombre `Carta`, arte vacío y puntajes `(650,350,500,500,500,500)` suman 3000, están en [250,750], tienen fortaleza 650 y debilidad 350. El constructor termina; consultar Fuerza devuelve 650 y Velocidad devuelve 350.

Alternativo: `(500,500,500,500,500,500)` cumple rango y suma pero falla fortaleza. `(650,351,499,500,500,500)` cumple rango, suma y fortaleza, pero todos sus valores superan 350: falla debilidad. `(750,249,501,500,500,500)` se rechaza por velocidad antes de evaluar la suma. Nombre solo espacios falla antes de asignar campos. Ejemplos contrastados por lectura y por los escenarios definidos en las pruebas, sin afirmar su ejecución en esta tarea.

## Errores, límites y efectos observables

No se admite null en nombre ni ASCII, pero los parámetros no anulables son una comprobación estática del compilador, no una barrera de ejecución; por eso hay guardas explícitas. No hay setters, persistencia, cancelación, asincronía ni transacciones. La clase no impone unicidad de nombre, límites de caracteres o tamaño del arte, ni reimplementa igualdad: se compara por referencia cuando se usa en las listas del modelo.

## Relaciones y verificación

[Jugador](Jugador.md) contiene referencias a las cartas y [Mazo](Mazo.md) las reparte. Combate consulta el atributo de sus cartas para determinar el resultado numérico; el desempate corresponde a Combate, no a este archivo.

[PersonajeTests.cs](../../test/proyecto-integrador-final-tests/PersonajeTests.md) define conservación de nombre/arte/seis atributos, umbrales inclusivos con arte vacío, siete combinaciones inválidas, tres variantes de nombre inválido y ASCII null. Las pruebas comprueban tipo de excepción, no todos sus mensajes ni parámetros; tampoco cubren directamente el enum fuera de rango. [Plan de pruebas](../../../plan-de-pruebas.md): la evidencia histórica no sustituye una ejecución de esta referencia.
