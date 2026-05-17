# ReunionWelcomeApp

Aplicación de escritorio Windows para evento de reunión de exempleados después de 20 años.

## Características

- **Dual-screen**: Pantalla de entrada (laptop) + Pantalla presentación (proyector)
- **Fallback automático**: Si no hay segundo monitor, abre segunda ventana redimensionable
- **Nube de nombres**: Nombres animados en pantalla de presentación
- **Persistencia**: Excel (.xlsx) y CSV - se actualiza en cada nombre
- **Recuperación**: Al iniciar, carga nombres existentes del CSV
- **Modo Demo**: Simulación automática para testing
- **Hotkeys completos**: Control total sin mouse

## Requisitos

- Windows 10/11 (64-bit)
- .NET 8 Runtime (incluido en build self-contained)

## Uso

1. Conectar laptop al proyector (opcional)
2. Ejecutar `ReunionWelcomeApp.exe`
3. Pantalla de entrada aparece en laptop
4. Presionar ENTER para comenzar a registrar nombres
5. Los nombres aparecen en la pantalla de presentación
6. Presionar ENTER again para enviar nombre

## Hotkeys

| Tecla | Función |
|-------|---------|
| ENTER | Iniciar registro / Enviar nombre (con focus automático) |
| ESC | Limpiar input |
| F1 | Mostrar debug info |
| F2 | Toggle fullscreen (pantalla input) |
| F3 | Toggle modo demo |
| F4 | Mostrar/Ocultar ventana de presentación |

## Modo Fallback (sin segundo monitor)

Si no detecta segundo monitor:
- Abre ventana de presentación redimensionable
- Se puede mover, redimensionar y cerrar con botón X
- F4 para mostrar/ocultar desde pantalla input

## Persistencia de Datos

- **CSV**: Se actualiza automáticamente con cada nombre
- **Excel**: Se genera junto con CSV
- **Recuperación**: Al iniciar, carga todos los nombres previos
- **Ubicación**: `data/asistentes.csv` y `data/asistentes.xlsx`

## Configuración

Editar `Resources/config.json` para personalizar:

```json
{
  "sounds": {
    "startInteraction": "Resources/Sounds/start.wav",
    "typingFeedback": "Resources/Sounds/typing.wav",
    "submissionConfirm": "Resources/Sounds/confirm.wav",
    "volume": 0.8
  },
  "images": {
    "logoGreen": "Resources/Images/Impsat ThinkAhead1.jpg",
    "logoBlue": "Resources/Images/Impsat-FondoBlanco.jpg"
  },
  "animations": {
    "nameDisplayDuration": 5000,
    "nameFadeInDuration": 500,
    "nameFadeOutDuration": 800
  },
  "nameCloud": {
    "maxNamesVisible": 50,
    "specialHighlightEvery": 10
  },
  "demo": {
    "enabled": false,
    "mockNamesCount": 100,
    "intervalMs": 3000
  }
}
```

## Estructura del Proyecto

```
ReunionWelcomeApp/
├── ReunionWelcomeApp.sln
├── src/ReunionWelcomeApp/ReunionWelcomeApp/
│   ├── Models/
│   │   ├── Attendee.cs          # Entidad asistente
│   │   └── AppConfig.cs         # Modelo de configuración
│   ├── Services/
│   │   ├── AudioService.cs      # Reproducción de sonidos
│   │   ├── ConfigService.cs    # Carga/guardado config JSON
│   │   ├── ExcelExportService.cs # Export Excel/CSV
│   │   ├── LoggingService.cs   # Logging centralizado
│   │   └── ScreenService.cs    # Detección dual-monitor
│   ├── ViewModels/
│   │   ├── InputViewModel.cs    # Lógica pantalla input
│   │   └── PresentationViewModel.cs # Lógica presentación
│   ├── Resources/
│   │   ├── config.json          # Configuración
│   │   ├── Sounds/             # Archivos de audio
│   │   └── Images/             # Logos
│   ├── MainWindow.xaml(.cs)    # Pantalla input
│   └── PresentationWindow.xaml(.cs) # Pantalla presentación
└── docs/
    ├── README.md
    ├── DESIGN.md
    └── AGENTS.md
```

## Estructura en Ejecución

```
ReunionWelcomeApp.exe
├── Resources/
│   ├── config.json
│   ├── Sounds/
│   └── Images/
└── data/
    ├── asistentes.xlsx
    └── asistentes.csv
```

## Solución de Problemas

**No detecta segunda pantalla:**
- Verificar conexión del proyector
- Usa modo fallback automáticamente (ventana redimensionable)
- Usar F4 para mostrar/ocultar presentación

**Sin sonido:**
- Verificar archivos en Resources/Sounds/
- Revisar config.json para rutas (debe ser Resources/Sounds/...)
- Los sonidos WAV deben ser formato PCM

**Excel no se genera:**
- Verificar permisos de escritura en carpeta data/
- CSV siempre se genera como backup

**La aplicación no guarda nombres al cerrar:**
- Los datos se guardan automáticamente en cada entrada
- Verificar carpeta data/ tiene permisos de escritura

## Build

```bash
# Desarrollo
dotnet run

# Release (single-file)
dotnet publish -c Release
```

El exe standalone estará en:
`bin/Release/net8.0-windows/win-x64/publish/ReunionWelcomeApp.exe`

## Licencia

Uso interno - Evento reunion de exempleados