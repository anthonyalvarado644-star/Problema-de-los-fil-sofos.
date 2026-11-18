# Informe Tecnico - Simulador Visual del Problema de los Filosofos Cenando

## 1. Introduccion

El Problema de los Filosofos Cenando es un problema clasico de sincronizacion en sistemas operativos, propuesto por Edsger W. Dijkstra en 1965. Se utiliza para ilustrar problemas de concurrencia como el deadlock (interbloqueo) y la inanicion (starvation).

Este proyecto implementa una solucion visual en C# con Windows Forms (.NET 9) que permite:
- Seleccionar entre 4 algoritmos de solucion diferentes.
- Ver graficamente el estado de cada filosofo en tiempo real.
- Controlar la velocidad de ejecucion para observar los procesos con detalle.
- Detectar y avisar visualmente cuando ocurre un deadlock.

## 2. Especificaciones tecnicas

| Elemento | Detalle |
|----------|---------|
| Lenguaje | C# |
| Framework | .NET 9.0 |
| Interfaz | Windows Forms (WinForms) |
| Tipo de aplicacion | Escritorio (WinExe) |
| Plataforma objetivo | Windows |
| Numero de filosofos | 5 fijos |
| Nombres de los filosofos | Aristoteles, Platon, Socrates, Confucio, Descartes |
| Estados posibles | Pensando, Hambriento, Comiendo |

## 3. Descripcion de la interfaz grafica

### 3.1 Panel superior (controles)
- Titulo de la aplicacion.
- Menu desplegable para elegir el algoritmo de solucion.
- TrackBar para ajustar la velocidad de simulacion (0.1x a 2.0x).
- Botones: Iniciar, Detener, Reiniciar, Salir.

### 3.2 Panel central (mesa grafica)
- Mesa circular grande en el centro con plato decorativo.
- 5 filosofos distribuidos en circulo alrededor de la mesa.
- Cada filosofo muestra:
  - Nombre.
  - Circulo coloreado segun su estado actual.
  - Borde rojo cuando esta comiendo.
  - Contador de comidas realizadas (xN).
- Cubiertos entre filosofos que se iluminan en verde cuando estan en uso.
- Lineas radios decorativas.

### 3.3 Panel inferior (estadisticas y leyenda)
- Leyenda de colores: azul (Pensando), amarillo (Hambriento), verde (Comiendo).
- Etiqueta de estado general con mensajes informativos y alertas de deadlock.
- Fila de estadisticas por filosofo con estado actual y contador de comidas, actualizada cada 50 ms.

### 3.4 Deteccion de deadlock
La aplicacion monitoriza continuamente si los 5 filosofos estan en estado Hambriento simultaneamente durante mas de 3 segundos. Si ocurre, muestra un mensaje en rojo:
DEADLOCK DETECTADO: todos estan hambrientos y ninguno puede comer.


## 5. Funcionamiento del control de velocidad

Un TrackBar permite ajustar un factor multiplicador (factorVelocidad) entre 0.1 y 2.0. Todos los tiempos de espera internos (Task.Delay) se dividen por este factor:

- 0.1x = 10 veces mas lento (ideal para observar detalle).
- 1.0x = velocidad normal.
- 2.0x = 2 veces mas rapido.

El valor se aplica en tiempo real sin necesidad de reiniciar la simulacion.

## 6. Optimizaciones graficas

Para evitar el parpadeo al redibujar la mesa cada 50 ms:
- Se creo una clase PanelDibujo heredada de Panel con ControlStyles.OptimizedDoubleBuffer, AllPaintingInWmPaint, UserPaint y ResizeRedraw.
- El formulario principal tambien tiene DoubleBuffered = true.

Esto garantiza una actualizacion fluida de los graficos sin parpadeos visibles.

## 7. Pasos para ejecutar

### Requisitos
- Windows 10 o 11.
- .NET SDK 9.0 instalado.

### Comandos
```bash
# 1. Clonar o descomprimir el proyecto
cd "d:\problemas de los filosofos"

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar directamente
dotnet run

# 4. O compilar y ejecutar el .exe
dotnet build -c Release
bin\Release\net9.0-windows\FilosofosGUI.exe
```

## 8. Conclusiones

La aplicacion permite comprender visualmente como diferentes estrategias de sincronizacion afectan al comportamiento del sistema:
- La jerarquia de recursos y el limite N-1 evitan el deadlock de forma determinista.
- El monitor por estados requiere notificacion adecuada entre filosofos para evitar inanicion.
- El deadlock intencional demuestra como una mala orden en la adquisicion de recursos provoca bloqueo total.

La interfaz grafica y el control de velocidad facilitan la observacion de estos fenomenos en tiempo real, haciendo el concepto accesible para fines educativos.

## 9. Estructura de archivos

```
d:\problemas de los filosofos\
├── FilosofosGUI.csproj   # Proyecto .NET 9 Windows Forms
├── Program.cs            # Codigo fuente (interfaz + 4 algoritmos)
├── README.md             # Instalacion y ejecucion
├── INFORME.md            # Este informe tecnico
├── bin\                  # Ejecutables compilados
└── obj\                  # Archivos intermedios de compilacion
```
## 4. Algoritmos implementados

### 4.1 Jerarquia de Recursos (Opcion 1)
Cada filosofo adquiere primero el cubierto de menor indice y luego el de mayor indice. Esto rompe la condicion de espera circular y evita deadlock.

### 4.2 Solucion por Estados / Monitor (Opcion 2)
Un monitor central controla el acceso. Un filosofo solo puede comer si:
1. Esta hambriento.
2. Ninguno de sus dos vecinos esta comiendo.

Si no puede comer, se bloquea en su semaforo privado hasta que un vecino termine y lo despierte.

### 4.3 Limite de Filosofos (N-1) (Opcion 3)
Solo permiten sentarse a la mesa N-1 (4) filosofos al mismo tiempo. Como siempre hay al menos un filosofo que puede tomar ambos cubiertos, no hay deadlock.

### 4.4 Deadlock Intencional (Opcion 4)
Todos los filosofos intentan tomar primero el cubierto izquierdo y luego el derecho, con una pequena demora forzada entre ambas acciones. Esto garantiza que eventualmente todos tomen el izquierdo y queden esperando el derecho, provocando un deadlock seguro.