[![Unity](https://img.shields.io/badge/Unity-2022.3%2B%20%7C%20Unity%206-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-9.0%2B-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Render Pipeline](https://img.shields.io/badge/URP-Universal%20Render%20Pipeline-4682B4?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/srp/Universal-Render-Pipeline)

# Quick Start
Auf dem Automaten `Fishing Game_Final` die AFishingGameDisplay.exe öffnen.
xrProject auf eine Meta Quest Bauen und neustarten.

# Neues Spiel
Nach jedem Spielstart beide Anwendungen neustarten (Desktop App Alt+F4).
# Unity Version & Setup

**Unity Version:** 6000.4.3f1

**Unity Hub**
Dieses Repository beinhaltet 2 Unity Projekte

In Unity Hub -> Add+ -> Add from Disk:

Für das Arcade Projekt:
`ExtendedRealityArcade/ArcadeDisplay/AFishingGameDisplay`

Für das XR Projekt:
`ExtendedRealtiyArcade/ArcadeXR/xrProjekt`


# The Catch Factory and Fish Scriptable Objects

The Catch Factory is Responsible for Instantiating catchable objects.
Collects all available data (ScriptableObjects) that is available.
**FOLDER:** `Assets/Resources/ScriptableObjects/...`

### HOW TO ADD A FISH PREFAB TO THE CATCH FACTORY

create the scriptable asset there with:
`Create -> Catch -> New catch Data`

assign your prefab fish into the given field

give it a name and description, and a spawn layer

--> now the fish will be recognized by the catch factory! 

---

# Fischmuster - Spline

**Basic:**
Fisch GameObjekt: Hat BezierprefabToolKit.cs
BezierprefabToolKit Script: Hat referenz auf die Spline szene!

**Beschreibung:**
Die fischmuster können durch das Bezierprefab toolkit und leere szenen erzeugt werden. 
Jene Szene, die eine menge an segmenten, und jedes segment punkte enthält, wird ein BSpline der Form C1, (Unity Hermetische Splines erzeugt.)

Um ein Fischmuster anzulegen: BezierPrefab Toolkit an ein GameObjekt. 
Spline Scene, wie oben beschrieben erstellt als referenz angeben.
Erstellen des SplineBase mit `makeSplineLocal()`

Der "SplineBase" als instanz kann verwendet werden um den ganzen spline entweder ganz zu bauen, als Linie mit einem Detailgrad, oder an einer position t in [0,1] auszuwerten. 

---

# VR-Controls

* **Linker Stick:** Bewegen
* **Rechter Stick:** Drehen
* **Rechter Grip-Button:** Halten um Wurf-Cast zu starten, beim auswerfen los lassen
* **Rechter Trigger:** Auswahl bei Netzwerk Einstellungen
* **A-Button:** 
  * Press: Fisch Fangen, wenn an der Boye ist; halten zum Einholen ohne Fisch; Während Mini-Game tippen oder halten um den Fisch im Kreis zu halten;
  * Nach Fang oder line break: Drücken zum Einholen (clip-in)

---

# Configs

### Network - Arcade
Einstellungen der Arcade werden in einer Textdatei gespeichert sobald auf SAVE gedrückt wird. Zum Übernehmen muss die Anwendung neu gestartet werden. Diese kann auch auf einem Text-Editor vor dem Spielstart geändert werden. Dabei ist der listenerPort, die des Arcade Systems und remotePort die der VR Brille.

### Network-Brille
Wenn keine Verbindung zur Arcade besteht, ist über dem Teich ein UI sichtbar. Mithilfe des rechten Controllers können die zugehörigen Adressen und Ports ausgewählt und mit dem Trigger des rechten Controllers eingetippt werden. Mit CONNECT werden die Einstellungen gespeichert und es wird erneut nach der Arcade Maschine gesucht.

### Arduino
LightPort und CoinPort werden in einer eigenen JSON bearbeitet. 

**COM-PORT bestimmen**
Unter Windows: Start->Geräte-Manager und unter "COM & LPT" den Arduino finden. In den Klammern steht der COM-Port.

Das Spiel funnktioniert auch ohne Arduino verbindung. Der Shop wandelt die Fische direkt in verwendbare Coins um.

### AR
In Unity ist auf der Kamera des xrPlayers keine Skybox ausgewählt, dies ist notwendig, um den Passthrough zu ermöglichen. Die Skybox wird unter `Window -> Ligthing -> Enviornment` gesetzt.

---

# Material Height reader
Extract Vertex Height from a GameObject with Material Setup as stated:

allows this class to read vertex data from a material
 
not that the RGB Channel Out of the material must 
pass the Vertex shader data into the channel
if `someMaterial.SetFloat("_IsHeightPass", 1f);`
and `someMaterial.SetFloat("_IsHeightPass", 0f);`
is possible, to switch the render pass to a vertex output
 
the camera always faces downward and is auto generated
 
**--- usuage inside the material ---**
-> create a value input of name "IsHeightPass"
-> create a branch which gets the output color (default) and vertex position, to be pasted.
-> the branch then pushes the output value of the branch into the color RGBA output.

---

# GameHandle & Player setup

Im Game Handle Script muss es verschiedene prefabs für den Player geben, es gibt dabei einen PlayerBase Shared, einen PC Controller und einen VR Controller prefab / bzw subklassen vom shared player.
das ist deshalb nötig weil für den jeweiligen player ein anderer transform, z.b. der der VR Hand und leeres transform im PC Player Returned wird. In beiden fällen muss aber der spieler die Angel halten. 

---

# Troubleshooting

**Wenn das projekt nicht startet:** 
Initial Asset Data Base Refresh - Problem gesichtet bei Mac Os: Projekt neu clonen

---

# Environment & Atmosphäre

---

## Wassersystem - Interaktive Wellen

**Architektur:** Custom URP Shader + RenderTexture-basierte Partikel-Interaktion

Die **interaktiven Wellen** funktionieren über ein separates Wave-Partikel-System, das in eine RenderTexture rendert:

1. Objekt mit `WaterObject` Script berührt Wasser
2. Wave-Partikel-System spawnt und rendert Kreise in RenderTexture (dedizierte Kamera)
3. Wasser-Shader sampelt diese RenderTexture via Parameter `Interactive Wave`
4. Shader deformiert die Wasser-Oberfläche an den Partikel-Positionen

**Wichtig:** Wave-Partikel (RenderTexture) ≠ Splash-Partikel (visuell)

### Shader-Parameter (nur relevant)
- `Interactive Wave`: RenderTexture für Partikel-Deformation
- `Interactive Wave Strength`: Deformations-Intensität
- `Wave Scale`: Wellenhöhe (0 = flach, 2.5 = turbulent)
- `Foam Color`, `Foam Speed`, `Foam Scale`: Schaum-Effekte
- Weitere (Metallic, Gloss, Normal, Absorption etc.): Standard-Effekte

---

## Nebelsystem - Distance-Fade Shader

**Architektur:** Partikelsystem + Custom Shader mit Entfernungs-Kontrolle

Der Fog-Shader ermöglicht **entfernungsbasiertes Fading** — Nebel nur zwischen definierten Entfernungen:

- Verhindert Nah-Clipping Artefakte
- Ermöglicht "Rand-Nebel" (Nebel nur am Horizont)
- Noise-Animation für organisches Aussehen

### Shader-Parameter
- `Fog Start`: Ab dieser Entfernung Nebel sichtbar
- `Fog End`: Bei dieser Entfernung vollständig opak
- `Fog Alpha`: Gesamt-Deckkraft
- `Fog Color`: Nebel-Farbe
- `Noise Tiling`, `Noise Speed`: Noise-Animation

### Setup
- Partikelsystem in Scene
- Fogshader auf Material
- Window > Rendering > Lighting > Environment: **Fog aktiviert**

---

## Skybox-Blend-Shader

**Architektur:** Dual-Cubemap Interpolation

Ein Shader, der zwischen zwei Cubemaps (Calm + Horror) lerpt — keine Szenen-Neuload:

### Shader-Parameter
- `_BlendAmount`: Interpolation 0-1 (0 = Calm, 1 = Horror)
- `_CalmCubemap`, `_HorrorCubemap`: Zwei Cubemaps
- `_Exposure`, `_Tint`, `_Rotation`: HDR & Effekte

### Setup
1. Material mit `SkyboxCubemapBlend.shader` erstellen
2. Beide Cubemaps + Parameter einstellen
3. Material als Skybox setzen (Window > Rendering > Lighting > Environment)
4. `HorrorSkyboxBlendController` animiert `_BlendAmount` (0 = Calm, 1 = Horror)

---

## Horror-Integration (alle Systeme)

### Basis
- Horror-Wert: 0-100% über Timer × 0,75 Sekunden
- Alle Controller abonnieren `HorrorManager.OnHorrorValueChanged`
- Erhalten normalisiert Horror (0-1) via `Apply(float t)`

### Controller
- `HorrorWaterController`: Wave Scale, Farben
- `HorrorFogController`: Density, Emissions
- `HorrorSkyboxBlendController`: `_BlendAmount`
- `HorrorLightController`: Lichfarbe/-stärke

### Upgrades
- Tiefe 1-3: Max Horror 33% → 100%
- Boss spawnt: Horror ≥ 95% **UND** Zeit ≤ 15 Sekunden

### Horror Changes
- Als Scriptable Objects erstellbar
- Im Scriptable Object und im Map Changer die selbe ID angeben
- Im HorrorManager dann adden. Diese Horror Changes werden dann beim Erreichen des Horror Wertes getriggered.

### Map Changer
- Skript den jeweiligen Objekten anhängen und Renderer/Audio zuweisen
- Bei Map Change Activator auf ein leeres Objekt anhängen und das zu aktivierende Objekt zuweisen

---

## Boss & Kreatur

### Boss-Spawn (Cthulhu)
- Condition: Horror ≥ 95% + Verbleibende Zeit ≤ 15s
- Abonniert `TimeManager.OnBossSpawnTime`

### Reh-Flucht
- Trigger: Gleiches Event wie Boss
- Animation: Essen → Rotation (180°) → Turning → Running
- Bewegung: Konstante Geschwindigkeit bis Timer = 0

---

## Debug

- **B-Taste:** Sofortiges Upgrade
- **Console:** Horror-Wert & Events geloggt
- **Play-Modus:** State-Transitions bei Horror ≥ 95%
