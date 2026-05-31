# Diseño de Arquitectura - ReunionWelcomeApp

## Visión General

Aplicación WPF .NET 8 con patrón MVVM para gestión de evento dual-screen con fallback automático y persistencia de datos.

## Arquitectura Global

```
┌─────────────────────┐     ┌──────────────────────┐
│   MainWindow        │     │  PresentationWindow │
│   (Input Screen)    │     │  (Proyector/Fallback)│
│                     │     │                      │
│  - Logo             │     │  - Logo              │
│  - Status text      │     │  - Current name      │
│  - Name input       │     │  - Name cloud        │
│  - Attendee count   │     │  - Attendee counter  │
│  - Hotkey handler   │────▶│  - Animations        │
└─────────────────────┘     └──────────────────────┘
         │                          │
         │ NameSubmitted            │
         ▼                          │
┌─────────────────────┐              │
│  InputViewModel    │              │
│  - InputName       │              │
│  - IsInputActive   │              │
│  - AttendeeCount   │              │
│  - IsDemoMode      │              │
│  - EnterPressed    │              │
│  - EscapePressed   │              │
│  - ToggleDemo      │              │
└─────────────────────┘              │
         │                           │
         │ LoadExistingAttendees     │
         ▼                           ▼
┌─────────────────────────────────────────┐
│           ExcelExportService             │
│  - SaveAttendees() → CSV + XLSX         │
│  - LoadAttendees() ← CSV                │
└─────────────────────────────────────────┘
```

## Componentes

### Modelos (`Models/`)

- **Attendee**: Entidad asistente
  - `Guid Id` - Identificador único
  - `string FullName` - Nombre completo
  - `DateTime ArrivalTime` - Hora de llegada

- **AppConfig**: Configuración raíz
  - `SoundConfig Sounds` - Rutas y volumen de sonidos
  - `ImageConfig Images` - Rutas de logos e imágenes
  - `AnimationConfig Animations` - Tiempos de animación
  - `NameCloudConfig NameCloud` - Configuración de nube
  - `DemoConfig Demo` - Configuración demo

### Servicios (`Services/`)

| Servicio | Responsabilidad | Métodos Clave |
|----------|-----------------|---------------|
| **LoggingService** | Logging centralizado | `Log()`, `LogError()`, `LogWarning()` |
| **ConfigService** | Carga/guardado JSON | `Load()`, `Save()`, `GetConfig()` |
| **AudioService** | Reproducción sonidos | `PlaySound()`, `SetVolume()` |
| **ExcelExportService** | Persistencia CSV/XLSX | `SaveAttendees()`, `LoadAttendees()` |
| **ScreenService** | Detección dual-monitor | `HasSecondaryScreen()`, `GetPresentationBounds()` |

### ViewModels (`ViewModels/`)

- **InputViewModel**: Lógica pantalla entrada
  - `InputName` - Texto del input
  - `IsInputActive` - Estado del input
  - `StatusMessage` - Mensaje de estado
  - `AttendeeCount` - Contador de asistentes
  - `IsDemoMode` - Estado demo
  - `EnterPressedCommand` - Comando ENTER
  - `EscapePressedCommand` - Comando ESC
  - `ToggleDemoCommand` - Toggle demo

- **PresentationViewModel**: Estado presentación
   - `CurrentName` - Nombre actual mostrado
   - `CurrentNameOpacity` - Opacidad para animación
   - `ShowCurrentName` - Visibilidad nombre actual
   - `AttendeeCount` - Contador
   - `NameCloud` - ObservableCollection de nombres
   - `AddName()` - Añadir nombre (ordena alfabéticamente y distribuye en grid)
   - `LoadExistingAttendees()` - Cargar previos
   - `ToggleDebug()` - Debug overlay

- **NameItem**: Item en la nube
  - `X`, `Y` - Posición
  - `Size` - Tamaño de fuente
  - `Opacity` - Opacidad
  - `Name` - Texto del nombre
  - `IsHighlight` - Highlight dorado

### Vistas

| Ventana | Propósito |
|---------|------------|
| **MainWindow** | Input Screen - Laptop |
| **PresentationWindow** | Presentation Screen - Proyector/Fallback |

## Decisiones Técnicas

| Decisión | Justificación |
|----------|---------------|
| WPF + .NET 8 | Mejor integración Windows, animaciones XAML nativas |
| MVVM | Separación UI/lógica, testabilidad, maintainable |
| ClosedXML | Excel sin Office instalado |
| System.Media.SoundPlayer | Sonidos simples, sin dependencias extras |
| Windows Forms Screen | Detección dual-monitor robusta |
| CSV como primario | Más confiable que Excel para recovery |
| Fallback automático | Funciona sin segundo monitor |

## Flujo de Datos

```
1. Usuario presiona ENTER en Input
      ↓
2. MainWindow.OnKeyDown → InputViewModel.EnterPressedCommand
      ↓
3. InputViewModel valida y emite NameSubmitted event
      ↓
4. MainWindow.OnNameSubmitted → PresentationWindow.ReceiveName()
      ↓
5. PresentationViewModel.AddName()
      ↓
6. PresentationViewModel guarda en ExcelExportService
      ↓
7. ExcelExportService.SaveAttendees() → CSV + XLSX
```

## Persistencia

### Guardado
- Se ejecuta en cada `AddName()`
- CSV: `data/asistentes.csv` (prioritario)
- XLSX: `data/asistentes.xlsx` (export)

### Carga al inicio
- `MainWindow.OnLoaded()` → `LoadExistingAttendees()`
- Lee CSV → `PresentationViewModel.LoadExistingAttendees()`
- Añade todos los nombres a la nube

## Excepciones y Robustez

- Global exception handler en `App.xaml.cs`
- Logging de todos los errores y eventos importantes
- Fallback silencioso en audio (sin crash)
- Fallback automático si no hay segundo monitor
- CSV siempre disponible como backup
- Ventana fallback redimensionable y cerrable

## Animaciones

- **Nombre actual**: Fade in → mostrar 5s → fade out
- **Nube de nombres**: Posición aleatoria, tamaño variable
- **Highlight especial**: Cada N nombres, dorado (10 por defecto)

## Hotkeys

| Tecla | Ubicación | Acción |
|-------|-----------|--------|
| ENTER | Input | Iniciar/Enviar |
| ESC | Input | Limpiar |
| F1 | Input/Pres | Debug |
| F2 | Input | Fullscreen |
| F3 | Input | Demo |
| F4 | Input | Toggle presentación |

## Notas de Implementación

- Name cloud usa distribución alfabética en formato de cuadrícula (2 o 4 columnas)
- Animaciones usando DispatcherTimer
- Demo mode genera nombres mock con timer configurable
- Fallback window: redimensionable, cerrable, movable
- Focus automático al input después de ENTER