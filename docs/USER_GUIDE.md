# Guía de Usuario - ReunionWelcomeApp

Aplicación de escritorio para eventos de reunion de exempleados. Muestra los nombres de asistentes en una "nube de nombres" animados en pantalla de presentación.

## Requisitos del Sistema

Ver archivo `REQUIREMENTS.md` para verificación automática de compatibilidad.

## Instalación

1. Copiar la carpeta `publish` completa al equipo
2. Asegurar que `Resources/` y `data/` estén junto al `.exe`
3. No mover archivos individualmente

Estructura requerida:
```
ReunionWelcomeApp.exe
├── Resources/
│   ├── config.json
│   ├── Sounds/
│   │   ├── start.wav
│   │   ├── typing.wav
│   │   ├── confirm.wav
│   │   ├── nameappear.mp3
│   │   ├── highlight.mp3
│   │   └── ambient.mp3
│   └── Images/
│       └── Impsat-FondoBlanco.jpg
└── data/  (se crea automáticamente)
```

**Nota**: La imagen de fondo solo aparece en la ventana de presentación (proyector), no en la ventana de entrada (laptop).

## Uso Básico

### Primera vez
1. Conectar laptop al proyector (si hay segundo monitor)
2. Ejecutar `ReunionWelcomeApp.exe`
3. La aplicación detecta automáticamente si hay segunda pantalla

### Flujo de Trabajo
1. **Pantalla de entrada** aparece en laptop (o monitor principal)
2. Escribir nombre del asistente
3. Presionar **ENTER** para enviar
4. El nombre aparece en la **pantalla de presentación**
5. Repetir para cada asistente

### Finalizar
- Simply close the application window
- All data is automatically saved
- On restart, existing names are loaded automatically

## Controles de Teclado

| Tecla | Acción |
|-------|--------|
| ENTER | Enviar nombre (cuando input tiene focus) |
| ESC | Limpiar campo de texto |
| F1 | Mostrar información de debug |
| F2 | Pantalla completa (input) |
| F3 | Activar/desactivar modo demo |
| F4 | Mostrar/ocultar ventana de presentación |

## Modo Demo

Activar con **F3** para simular arrival de asistentes automáticamente. Útil para pruebas sin needing real attendees.

## Modo Fallback

Si no se detecta segundo monitor:
- La ventana de presentación se abre como ventana redimensionable
- Se puede mover, redimensionar y cerrar
- Funciona igual que en modo dual-screen

## Archivos de Datos

- **CSV**: `data/asistentes.csv` - se actualiza con cada nombre
- **Excel**: `data/asistentes.xlsx` - exportación paralela
- **Recuperación**: Al iniciar, carga todos los nombres previos automáticamente

---

## Configuración - config.json

El archivo `config.json` controla todos los aspectos visuales y de comportamiento. located in `Resources/config.json`.

### Sección: sounds

```json
"sounds": {
  "start": "Resources/Sounds/start.wav",
  "typing": "Resources/Sounds/typing.wav",
  "confirm": "Resources/Sounds/confirm.wav",
  "nameAppear": "Resources/Sounds/nameappear.mp3",
  "nameHighlight": "Resources/Sounds/highlight.mp3",
  "ambient": "Resources/Sounds/ambient.mp3",
  "ambientVolume": 0.3,
  "effectsVolume": 0.8
}
```

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| start | string | Sonido al iniciar interacción |
| typing | string | Sonido mientras escribe |
| confirm | string | Sonido al enviar nombre |
| nameAppear | string | Sonido cuando aparece nombre (MP3) |
| nameHighlight | string | Sonido cada 10 nombres (MP3) |
| ambient | string | Música de fondo (MP3) |
| ambientVolume | double | Volumen música fondo (0.0 a 1.0) |
| effectsVolume | double | Volumen efectos (0.0 a 1.0) |

### Sección: images

```json
"images": {
  "logoBlue": "Resources/Images/Impsat-FondoBlanco.jpg"
}
```

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| logoBlue | string | Logo en pantalla de presentación |

### Sección: animations

```json
"animations": {
  "nameDisplayDuration": 5000,
  "nameFadeInDuration": 500,
  "nameFadeOutDuration": 800
}
```

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| nameDisplayDuration | int | Ms que el nombre actual permanece visible |
| nameFadeInDuration | int | Ms de animación de entrada |
| nameFadeOutDuration | int | Ms de animación de salida |

### Sección: nameCloud

```json
"nameCloud": {
  "maxNamesVisible": 50,
  "specialHighlightEvery": 10,
  "exclusionRadiusPercent": 25,
  "nameColor": "#1A237E",
  "nameStrokeColor": "#FFFFFF",
  "nameStrokeWidth": 2,
  "nameMinSize": 18,
  "nameMaxSize": 36,
  "counterColor": "#003399"
}
```

| Parámetro | Tipo | Rango | Descripción |
|-----------|------|-------|-------------|
| maxNamesVisible | int | 1-200 | Máximo de nombres visibles en pantalla |
| specialHighlightEvery | int | 1-100 | Cada cuántos nombres se destacan en dorado |
| exclusionRadiusPercent | int | 0-50 | % del ancho de pantalla excluded del centro para nombres |
| nameColor | string | hex | Color principal del texto (ej: #1A237E = azul oscuro) |
| nameStrokeColor | string | hex | Color del contorno/halo |
| nameStrokeWidth | double | 0-10 | Ancho del contorno |
| nameMinSize | int | 8-72 | Tamaño mínimo de fuente |
| nameMaxSize | int | 8-72 | Tamaño máximo de fuente |
| counterColor | string | hex | Color del contador de asistentes |

**Colores disponibles**: Cualquier color hex válido como #RRGGBB
- #000000 = negro, #FFFFFF = blanco
- #1A237E = azul oscuro, #FF0000 = rojo
- #00FF00 = verde, #FFFF00 = amarillo

### Sección: demo

```json
"demo": {
  "enabled": false,
  "mockNamesCount": 100,
  "intervalMs": 3000
}
```

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| enabled | bool | Iniciar en modo demo al arrancar |
| mockNamesCount | int | Cuántos nombres generar |
| intervalMs | int | Ms entre cada nombre automático |

## Personalización Típica

### Ajustar tamaño de nombres
```json
"nameCloud": {
  "nameMinSize": 24,
  "nameMaxSize": 48
}
```

### Cambiar colores
```json
"nameCloud": {
  "nameColor": "#FF6600",
  "counterColor": "#FF0000"
}
```

### Sin exclusión central
```json
"nameCloud": {
  "exclusionRadiusPercent": 0
}
```

### Más nombres en pantalla
```json
"nameCloud": {
  "maxNamesVisible": 100
}
```

## Solución de Problemas

**No aparece la segunda pantalla:**
- Verificar conexión del proyector
- Presionar F4 para mostrar/ocultar presentación

**No hay sonido:**
- Verificar que archivos .wav existan en Resources/Sounds/
- Ajustar volumen en config.json

**Los datos no persisten:**
- Verificar que carpeta data/ tenga permisos de escritura
- CSV siempre se genera como respaldo

**La aplicación no inicia:**
- Verificar requisitos en REQUIREMENTS.md
- Revisar logs/app.log para errores

## Contacto

Para soporte técnico, revisar los logs en: `logs/app.log`