# Requisitos del Sistema

## Verificación Automática

Para verificar si tu equipo cumple los requisitos, ejecuta el script de verificación:

```powershell
.\check-requirements.ps1
```

O manualmene revisa los siguientes puntos.

---

## Requisitos Mínimos

| Requisito | Versión Mínima | Notas |
|-----------|----------------|-------|
| Sistema Operativo | Windows 10 (64-bit) | También compatible con Windows 11 |
| Arquitectura | x64 (64-bit) | No funciona en 32-bit |
| Memoria RAM | 2 GB | Recomendado 4 GB |
| Espacio en Disco | 200 MB | Para el ejecutable y recursos |

## Requisitos de Software

| Componente | Requerido | Notas |
|------------|-----------|-------|
| .NET 8 Runtime | No (incluido) | La aplicación es self-contained |
| Runtime de C++ | No necesario | No se usa |
| Excel (opcional) | 2010+ | Solo si se necesita abrir .xlsx manualmente |

## Requisitos de Pantalla

| Configuración | Recomendado | Mínimo |
|---------------|-------------|--------|
| Monitores | 2 | 1 (fallback) |
| Resolución monitor principal | 1280x720 | 800x600 |
| Resolución monitor secundario | 1024x768 | 800x600 |

### Modo Dual-Screen
- Conectar proyector/monitor adicional como extensión (no克隆)
- La aplicación detecta automáticamente el segundo monitor
- Si no hay segundo monitor, usa modo fallback (ventana redimensionable)

## Requisitos de Archivos

### Estructura de Archivos Requerida
```
ReunionWelcomeApp.exe
├── Resources/
│   ├── config.json          (requerido)
│   ├── Sounds/
│   │   ├── start.wav        (requerido)
│   │   ├── typing.wav       (requerido)
│   │   ├── confirm.wav      (requerido)
│   │   ├── nameappear.mp3   (requerido)
│   │   ├── highlight.mp3    (requerido)
│   │   └── ambient.mp3      (requerido)
│   └── Images/
│       └── Impsat-FondoBlanco.jpg (requerido)
```

### Permisos Requeridos
- Lectura en carpeta de la aplicación
- Escritura en subcarpeta `data/` (se crea automáticamente)
- Escritura en subcarpeta `logs/` (se crea automáticamente)

---

## Verificación Manual

### 1. Sistema Operativo
```cmd
winver
```
Debe mostrar Windows 10 o Windows 11.

### 2. Arquitectura
```cmd
echo %PROCESSOR_ARCHITECTURE%
```
Debe mostrar `AMD64`.

### 3. Memoria
```cmd
systeminfo | findstr /C:"Total Physical Memory"
```
Debe mostrar al menos 2 GB (2048 MB).

### 4. Resolución de Pantalla
```
Configuración > Sistema > Pantalla > Resolución
```
Debe ser al menos 1280x720 para monitor principal.

---

## Problemas Comunes

### "La aplicación no se ejecuta"
- Verificar que sea Windows 64-bit
- Ejecutar como administrador la primera vez

### "No detecta segundo monitor"
- Verificar que el proyector esté conectado y Encendido
- En Windows: Configuración > Sistema > Pantalla > Detectar
- Verificar que esté en modo "Extender" (no "Duplicar")

### "Error al escribir datos"
- Verificar permisos de escritura en carpeta data/
- Crear la carpeta data/ manualmente si no existe

### "Sonido no funciona"
- Verificar que archivos WAV existan
- Verificar que el volumen del sistema no esté muteado

---

## Script de Verificación Automática

El script `check-requirements.ps1` verifica:
- Versión de Windows (10/11)
- Arquitectura (64-bit)
- Memoria RAM (mínimo 2GB)
- Espacio en disco (mínimo 200MB)
- Archivos requeridos existentes

Ejecutar en PowerShell:
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -File .\check-requirements.ps1
```

---

## Configuración Recomendada

### Laptop + Proyector
1. Conectar proyector por HDMI/VGA
2. En Windows: Win+P > "Extender"
3. Arrastrar ventana de presentación al segundo monitor
4. Ejecutar aplicación

### Solo un Monitor
- La aplicación funciona en modo fallback
- La ventana de presentación se abre redimensionable
- Usar F4 para mostrar/ocultar

### Optimización Visual
- Resolución del proyector: 1280x720 o superior
- Fondo de pantalla del proyector: negro (para mejor contraste)
- Apagar protector de pantalla durante el evento