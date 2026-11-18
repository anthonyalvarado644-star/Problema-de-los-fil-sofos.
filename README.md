# Problema-de-los-Filosofos

Trabajo calificado de Sistemas Operativos II, Ing. en Sistemas. En la presente se desarrolla el problema de la cena de los filosofos resuelto en Lenguaje C#, empleando Windows Forms para la interfaz grafica y semaforos para el control de concurrencia.

Integrantes: Jesus Ernesto Ramirez Ruiz - 28.661.648 Anthony Jose Alvarado - 28.759.193

La Cena de los Filosofos es una metafora sobre 5 procesos (filosofos) que compiten por 5 recursos compartidos (cubiertos) en una mesa redonda. Cada uno necesita dos cubiertos simultaneamente para comer, pero si todos toman el de su izquierda al mismo tiempo, ninguno podra tomar el de su derecha y el sistema se congela por completo (interbloqueo o deadlock).

Por que es importante comprenderlo: Ensenia a coordinar multiples tareas que se ejecutan en paralelo sobre recursos limitados (memoria, archivos, bases de datos). Dominar su solucion (mediante semaforos y asimetria) garantiza tres cosas en el desarrollo de software real:

Evitar bloqueos: Impide que los programas se queden congelados esperando recursos retenidos por otros procesos.

Proteger los datos: Asegura que dos procesos no modifiquen la misma informacion al mismo tiempo, evitando corrupcion.

Garantizar equidad: Permite que todos los procesos avancen sin quedar relegados indefinidamente (inanicion).

Guia paso a paso: Desarrollo en C# sobre Windows con .NET 9

- Paso 1: Crear el entorno en terminal Abre la terminal en Windows (PowerShell o CMD) y crea un directorio limpio para trabajar:

Bash
mkdir proyecto_filosofos
cd proyecto_filosofos

- Paso 2: Clonar el repositorio Desde GitHub, trae el proyecto a tu maquina local usando tu Token de Acceso Personal (PAT):

Bash
git clone https://Nombre_de_Usuario:TU_TOKEN_AQUI@github.com/TU_USUARIO/Problema-de-los-Filosofos.git
cd Problema-de-los-Filosofos

Como generas un Token de Acceso Personal (PAT)? Dentro de GitHub ir a tu cuenta y proceder con los siguientes pasos: Usuario / Configuracion / Opciones de Desarrollador / Tokens de Acceso Personal / Tokens (clasico) / Habilitar primera opcion repo. Hecho esto tendras habilitado un codigo temporal que facilita el acceso para la migracion del repositorio.

- Paso 3: Instalar .NET SDK 9.0 Descarga e instala .NET SDK desde: https://dotnet.microsoft.com/download

Verifica la instalacion en la terminal:

Bash
dotnet --version

Debe mostrar una version igual o superior a 9.0.100.

Metodos de solucion disponibles

| Opcion | Metodo | Descripcion |
|--------|--------|-------------|
| 1 | Jerarquia de Recursos | Evita deadlock ordenando la adquisicion de cubiertos (primero el de menor indice). |
| 2 | Solucion por Estados (Monitor) | Monitor central que solo permite comer si ambos vecinos no estan comiendo. |
| 3 | Limite de Filosofos (N-1) | Solo permiten sentarse a la mesa N-1 filosofos a la vez, evitando deadlock. |
| 4 | Deadlock Intencional | Todos toman el izquierdo primero y esperan al derecho -> provoca deadlock forzado. |

Cada filosofo muestra su estado con color:

- Azul: Pensando
- Amarillo: Hambriento
- Verde: Comiendo

El cubierto entre dos filosofos se ilumina en verde cuando esta en uso.

Verificacion empirica y entrega

Inspecciona el comportamiento de la simulacion:

1. Inicia cada uno de los 4 metodos y anota si ocurren deadlocks.
2. Ajusta el TrackBar de velocidad (0.1x a 2.0x) para observar los procesos con detalle.
3. Verifica que el panel inferior muestre el estado y contador de comidas de cada filosofo en tiempo real.
4. Confirma que aparece el aviso rojo DEADLOCK DETECTADO cuando todos quedan hambrientos.

Guarda el archivo, registra tus cambios en el historial de Git y sube el trabajo final al repositorio remoto:

Bash
git add .
git commit -m "Simulador visual de filosofos con GUI, informe y documentacion completa"
git push origin main

Requisitos previos

- .NET SDK 9.0 o superior (verificado con dotnet --version)
- Sistema operativo: Windows (requerido por Windows Forms)

Estructura del repositorio

```
Problema-de-los-Filosofos/
├── FilosofosGUI.csproj   # Proyecto .NET 9 Windows Forms
├── Program.cs            # Codigo fuente completo
├── README.md             # Instrucciones y guia de trabajo
├── INFORME.md            # Informe tecnico y de funcionamiento
├── bin\                  # Binarios compilados (generados)
└── obj\                  # Archivos temporales de compilacion (generados)
```

Notas

- Advertencias de nulabilidad (CS8618, CS8622) son propias del compilador C# con nullable enabled y no afectan el funcionamiento.
- La aplicacion requiere interfaz grafica, por lo que debe ejecutarse en Windows con entorno de escritorio.

- Paso 4: Abrir el proyecto en Visual Studio Code Desde la carpeta del proyecto, ejecuta:

Bash
code .

Dentro del archivo, veras los siguientes elementos principales:

- FilosofosGUI.csproj - Proyecto .NET 9 Windows Forms.
- Program.cs - Codigo fuente completo (interfaz grafica + 4 algoritmos de concurrencia).
- README.md - Este archivo con instrucciones.
- INFORME.md - Informe tecnico y de funcionamiento completo.

- Paso 5: Compilar y ejecutar Desde la terminal, dentro de la carpeta del proyecto:

Bash
dotnet build -c Release

O ejecuta directamente sin compilar por separado:

Bash
dotnet run

O ejecuta el ejecutable generado directamente:

Bash
bin\Release\net9.0-windows\FilosofosGUI.exe

Deten la ejecucion en cualquier momento cerrando la ventana de la aplicacion.