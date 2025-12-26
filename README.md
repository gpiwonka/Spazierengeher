# Spazierengeher

Eine Cross-Platform Schrittzähler-App, entwickelt mit .NET 10 und .NET MAUI.

## Über das Projekt

Diese App ist entstanden, weil ich gerne spazieren gehe und das neue Visual Studio 2026 mit .NET 10 testen wollte. Mir ist halt nichts anderes eingefallen. 🤷‍♂️
Daher: Keine großen Ambitionen, keine Business-Pläne, nur eine funktionale App zum Schritte zählen beim Spazierengehen.

## Features

### Kernfunktionen
- **Schrittzählung** - Automatisches Tracking deiner täglichen Schritte
- **Höhenmeter-Tracking** - Erfasst Höhenunterschiede während deiner Aktivitäten
- **Distanzberechnung** - Berechnet die zurückgelegte Strecke basierend auf deiner Schrittlänge
- **Tägliche Ziele** - Setze individuelle Tagesziele für Schritte und Höhenmeter
- **Aktivitätszeit** - Geschätzte aktive Zeit basierend auf deinen Schritten
- **Verlauf** - Detaillierte Historie deiner täglichen Aktivitäten

### Technische Features
- **Hintergrund-Tracking** - Kontinuierliches Tracking auch wenn die App geschlossen ist
- **Foreground Service** (Android) - Persistente Benachrichtigung für zuverlässiges Tracking
- **Auto-Start** - Optional automatischer Start beim Geräte-Boot
- **Lokale Datenspeicherung** - SQLite-Datenbank für schnellen Zugriff und Offline-Funktionalität
- **Echtzeit-Updates** - Live-Aktualisierung der Schrittzahl in der UI
- **Berechtigungsverwaltung** - Intelligente Handhabung von Sensor- und Standort-Berechtigungen

## Technologie-Stack

### Framework & Runtime
- **.NET 10** - Neueste Version des .NET Frameworks
- **.NET MAUI** - Multi-platform App UI Framework für Cross-Platform-Entwicklung
- **Blazor Hybrid** - Moderne Web-UI-Technologie in nativen Apps

### Entwicklungsumgebung
- **Visual Studio 2026** (oder neuer)


### Datenbank & Persistenz
- **SQLite** - Leichtgewichtige lokale Datenbank
- **sqlite-net-pcl** (Version 1.9.172) - .NET SQLite-Wrapper

### UI-Framework
- **Bootstrap 5** - Responsive Design-Framework
- **Razor Components** - Komponentenbasierte UI-Architektur

### Plattform-spezifische Features
- **Android**
  - Activity Recognition API
  - Foreground Service
  - Boot Receiver für Auto-Start
  - Persistente Benachrichtigungen

- **iOS**
  - Core Motion Framework
  - Background Modes (Location Updates)
  - Motion & Fitness Berechtigungen

## Projektstruktur

```
Spazierengeher/
├── Components/
│   ├── Layout/          # Layout-Komponenten (NavMenu, MainLayout)
│   └── Pages/           # Razor Pages (Home, History, Settings)
├── Data/                # Datenmodelle und Datenbankzugriff
│   ├── Models.cs        # Datenmodelle (DailySteps, UserSettings)
│   └── DailyStepsDb.cs  # SQLite-Datenbankkontext
├── Services/            # Business-Logik
│   ├── IStepCounterService.cs
│   ├── BlazorStepCounterService.cs
│   └── Platform-spezifische Implementierungen
├── Platforms/           # Plattform-spezifischer Code
│   ├── Android/
│   │   └── Services/    # Android Foreground Service, BootReceiver
│   └── iOS/
│       └── Services/    # iOS Core Motion Service
└── Resources/           # App-Ressourcen (Icons, Fonts, etc.)
```

## Installation & Build

### Voraussetzungen
- Visual Studio 2026
- .NET 10 SDK
- Für Android: Android SDK 24.0 oder höher
- Für iOS: Xcode 15.0+, macOS mit Apple Development Account

### Build-Schritte

1. **Repository klonen**
   ```bash
   git clone https://github.com/dein-username/Spazierengeher.git
   cd Spazierengeher
   ```

2. **Projekt öffnen**
   - Öffne `Spazierengeher.slnx` in Visual Studio 2022

3. **Zielplattform wählen**
   - Wähle in der Toolbar die gewünschte Plattform:
     - `net10.0-android` für Android
     - `net10.0-ios` für iOS
     - `net10.0-windows` für Windows

4. **Build & Deploy**
   - Drücke F5 oder klicke auf "Start Debugging"
   - Für Release-Build: Build → Publish → Zielplattform auswählen

### Plattform-spezifische Anforderungen

#### Android
```xml
<uses-permission android:name="android.permission.ACTIVITY_RECOGNITION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED" />
```

#### iOS
```xml
<key>NSMotionUsageDescription</key>
<string>Wir benötigen Zugriff auf den Bewegungssensor, um deine Schritte zu zählen.</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>Wir verwenden deinen Standort, um Höhenmeter zu berechnen.</string>
```

## Verwendung

### Erste Schritte
1. **Berechtigungen erteilen** - Beim ersten Start die erforderlichen Berechtigungen gewähren
2. **Ziel festlegen** - Setze dein tägliches Schrittziel (Standard: 10.000 Schritte)
3. **Tracking starten** - Die App startet automatisch das Tracking

### Features im Detail

#### Home-Screen
- **Aktuelle Schritte** - Große Anzeige deiner heutigen Schritte
- **Fortschrittsbalken** - Visueller Fortschritt zum Tagesziel
- **Statistiken** - Höhenmeter, Distanz und aktive Zeit
- **Zielanpassung** - Schnelle Anpassung des Tagesziels

#### Historie
- **Tägliche Übersicht** - Liste aller vergangenen Tage
- **Detaillierte Statistiken** - Schritte, Distanz und Höhenmeter pro Tag
- **Trend-Analyse** - Visualisierung deiner Fortschritte

#### Einstellungen
- **Tracking-Steuerung** - Start/Stop des Hintergrund-Trackings
- **Personalisierung** - Gewicht, Schrittlänge, Ziele
- **Benachrichtigungen** - Benachrichtigungseinstellungen
- **Auto-Start** - Automatisches Tracking beim Boot

## Implementierungs-Highlights

### Blazor Hybrid UI
Die App nutzt Blazor Hybrid, um eine moderne, reaktive Web-UI in eine native App zu integrieren:
- **Razor Components** für komponentenbasierte Entwicklung
- **Event-getriebene Updates** für Echtzeit-Aktualisierung
- **State Management** mit Dependency Injection

### Plattform-Integration

#### Android Foreground Service
```csharp
public class StepCounterForegroundService : Service
{
    // Persistente Benachrichtigung für zuverlässiges Hintergrund-Tracking
    // Automatische Sensor-Registrierung und Datenerfassung
    // Periodisches Speichern in SQLite-Datenbank
}
```

#### iOS Core Motion
```csharp
public class StepCounterService : IStepCounterService
{
    // CMPedometer für präzise Schrittzählung
    // CMAltimeter für Höhenmeter-Tracking
    // Background Mode für kontinuierliches Tracking
}
```

### Datenmodell
```csharp
public class DailySteps
{
    public DateTime DateKey { get; set; }
    public int Steps { get; set; }
    public int AltitudeMeters { get; set; }
}

public class UserSettings
{
    public int DailyGoal { get; set; }
    public double StepLengthMeters { get; set; }
    public bool AutoStartTracking { get; set; }
}
```

## Roadmap

- [ ] Mehrsprachigkeit (Deutsch/Englisch)
- [ ] Widget-Support
- [ ] Export-Funktionen (CSV, GPX)
- [ ] Soziale Features (Challenges, Freunde)
- [ ] Apple Health / Google Fit Integration
- [ ] Wear OS / Apple Watch App

## Lizenz

Dieses Projekt wurde zu Lern- und Demonstrationszwecken erstellt.

## Entwickelt mit

- .NET MAUI - [https://dotnet.microsoft.com/apps/maui](https://dotnet.microsoft.com/apps/maui)
- Blazor - [https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- SQLite - [https://www.sqlite.org/](https://www.sqlite.org/)

---

**Hinweis:** Diese App ist ein Showcase-Projekt für .NET 10 und .NET MAUI Entwicklung.
