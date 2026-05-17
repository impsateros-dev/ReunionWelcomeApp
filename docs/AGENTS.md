# Guía para Agentes IA - Extensiones Futuras

## Estructura del Proyecto

```
ReunionWelcomeApp/
├── ReunionWelcomeApp.sln
├── src/ReunionWelcomeApp/ReunionWelcomeApp/
│   ├── Models/
│   │   ├── Attendee.cs              # Entidad asistente (Id, FullName, ArrivalTime)
│   │   └── AppConfig.cs             # Config con sub-clases (Sound, Image, Animation, NameCloud, Demo)
│   ├── Services/
│   │   ├── AudioService.cs          # SoundPlayer - PlaySound(), PlayStartInteraction(), etc.
│   │   ├── ConfigService.cs        # Load()/Save() JSON - GetConfig()
│   │   ├── ExcelExportService.cs   # SaveAttendees()/LoadAttendees() - CSV y XLSX
│   │   ├── LoggingService.cs       # Log()/LogError()/LogWarning() - archivo app.log
│   │   └── ScreenService.cs        # HasSecondaryScreen()/GetPresentationBounds()/RotateToNextScreen()
│   ├── ViewModels/
│   │   ├── InputViewModel.cs        # InputName, IsInputActive, EnterPressedCommand, ToggleDemo
│   │   ├── PresentationViewModel.cs # NameCloud, AddName(), LoadExistingAttendees()
│   │   └── RelayCommand.cs         # Implementación simple de ICommand
│   ├── Resources/
│   │   ├── config.json              # Configuración externalizada
│   │   ├── Sounds/                  # start.wav, typing.wav, confirm.wav
│   │   └── Images/                  # Logos (Impsat*.jpg)
│   ├── MainWindow.xaml(.cs)        # Input Screen - Keyboard handling, F1-F4
│   └── PresentationWindow.xaml(.cs) # Presentation Screen - Name cloud, animations
└── docs/
```

## Arquitectura

### Flujo de Datos

```
MainWindow (Input)
    ↓ OnEnterPressed
InputViewModel.EnterPressedCommand
    ↓ NameSubmitted event
MainWindow.OnNameSubmitted
    ↓ ReceiveName()
PresentationWindow.ReceiveName
    ↓ AddName()
PresentationViewModel.AddName
    ↓ SaveAttendees()
ExcelExportService.SaveAttendees
    → data/asistentes.csv + .xlsx
```

### Persistencia

- CSV: `data/asistentes.csv` - se actualiza en cada nombre
- Excel: `data/asistentes.xlsx` - export con ClosedXML
- Carga automática al iniciar: `LoadExistingAttendees()`

## Agregar Nueva Funcionalidad

### 1. Agregar parámetro de configuración

Editar `Models/AppConfig.cs` y agregar propiedad en la clase correspondiente:

```csharp
public class AnimationConfig
{
    public int NameDisplayDuration { get; set; } = 5000;
    // Agregar nuevo parámetro
    public int NewParameter { get; set; } = 100;
}
```

Editar `Resources/config.json` con el nuevo valor:

```json
{
  "animations": {
    "nameDisplayDuration": 5000,
    "newParameter": 100
  }
}
```

### 2. Crear nuevo servicio

Ubicación: `Services/NuevoServicio.cs`

```csharp
using System;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp.Services;

public static class NuevoServicio
{
    public static void Metodo()
    {
        LoggingService.Log("Nuevo servicio iniciado");
    }
}
```

### 3. Agregar funcionalidad a ViewModel

Editar `ViewModels/InputViewModel.cs` o `PresentationViewModel.cs`:

```csharp
public class MiViewModel : INotifyPropertyChanged
{
    private string _nuevaPropiedad = string.Empty;
    
    public string NuevaPropiedad
    {
        get => _nuevaPropiedad;
        set { _nuevaPropiedad = value; OnPropertyChanged(); }
    }
    
    public void NuevoMetodo()
    {
        NuevaPropiedad = "nuevo valor";
    }
}
```

### 4. Actualizar UI (XAML)

Editar `MainWindow.xaml` o `PresentationWindow.xaml`:

```xml
<TextBlock Text="{Binding NuevaPropiedad}" />
```

### 5. Agregar hotkey

Editar `MainWindow.xaml.cs` en `OnKeyDown`:

```csharp
else if (e.Key == Key.F5)
{
    // Nueva funcionalidad
}
```

## Testing

```bash
# Desarrollo - ejecutar
cd src/ReunionWelcomeApp/ReunionWelcomeApp
dotnet run

# Compilar
dotnet build

# Testear con modo demo (F3)
# Testear fallback sin segundo monitor
```

## Build Producción

```bash
dotnet publish -c Release
```

El exe standalone estará en:
`bin/Release/net8.0-windows/win-x64/publish/ReunionWelcomeApp.exe`

## Recursos Disponibles

- **Logos**: `Resources/Images/Impsat*.jpg`
- **Config**: `Resources/config.json`
- **Datos**: `data/` (generado en runtime)
- **Logs**: `logs/app.log`

## Notas Importantes

### UI Framework
- Usar siempre `System.Windows` (no Windows Forms) para UI WPF
- Para dialogs: `System.Windows.MessageBox`
- Para timers: `System.Windows.Threading.DispatcherTimer`

### Sonidos
- Formato: WAV PCM
- Ubicación: `Resources/Sounds/`
- Rutas en config: `Resources/Sounds/archivo.wav`

### Imágenes
- Formato: JPG/PNG
- Ubicación: `Resources/Images/`
- Rutas en config: `Resources/Images/archivo.jpg`

### Logging
- Siempre usar `LoggingService.Log()` para tracking
- Usar `LoggingService.LogError()` para excepciones
- Revisar `logs/app.log` para debugging

### Persistencia
- CSV es más confiable que Excel para recovery
- Siempre guardar después de cada nombre
- Cargar existentes al iniciar aplicación

## Hotkeys Disponibles

| Tecla | Ubicación | Función |
|-------|-----------|---------|
| ENTER | Input | Iniciar / Enviar nombre |
| ESC | Input | Limpiar input |
| F1 | Input | Debug info |
| F2 | Input | Toggle fullscreen input |
| F3 | Input | Toggle modo demo |
| F4 | Input | Toggle presentación |
| F5 | Input | Rotar pantalla de presentación |
| ESC | Fallback | (solo limpia input) |
| F1 | Presentación | Toggle debug |

## Common Tasks

### Cambiar logo
1. Copiar nuevo logo a `Resources/Images/`
2. Actualizar `config.json` -> `images.logoBlue`

### Cambiar sonidos
1. Copiar WAV a `Resources/Sounds/`
2. Actualizar paths en `config.json`

### Ajustar animaciones
Editar `config.json` -> `animations`:
- `nameDisplayDuration`: ms que se muestra el nombre
- `nameFadeInDuration`: ms de entrada
- `nameFadeOutDuration`: ms de salida

### Ajustar nube de nombres
Editar `config.json` -> `nameCloud`:
- `maxNamesVisible`: límite de nombres en pantalla
- `specialHighlightEvery`: cada cuántos nombres hace highlight dorado

### Rotar pantalla de presentación
El servicio `ScreenService.RotateToNextScreen()` cicla entre todas las pantallas disponibles.
Al usar F5, la ventana de presentación se mueve a la siguiente pantalla en secuencia.
El índice se resetea cuando no hay segunda pantalla detectada.

### Auto-cierre de presentación
Al cerrar la ventana de entrada (MainWindow), si la ventana de presentación está visible, se cierra automáticamente.
Implementado en `MainWindow.OnClosed()`.