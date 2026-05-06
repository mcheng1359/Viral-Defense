# 🦠 Viral Defense

A first-person tower defense game built with **Unity** and **C#**, where players must strategically place turrets and engage enemies directly to stop waves of viruses from breaching their base.

---

## 🎮 About

Viral Defense puts you on the front lines of a biological war. Armed with a personal weapon and a limited budget of **Calories**, you must construct a defensive line of turrets across buildable tiles before enemy viruses reach your base. Each wave grows larger and stronger — survive all of them and eliminate the final virus to win.

---

## 🕹️ Controls

| Action | Input |
| --- | --- |
| Look Around | Mouse |
| Move | `W` `A` `S` `D` |
| Shoot | `Left Click` |
| Switch Gun / Build Mode | `E` |
| Select / Place Turret | `Left Click` (on tile) |
| Deselect Tile | `Right Click` / `Escape` |

---

## ⚔️ How to Win

- **Survive all waves** without your base health reaching **0**
- **Defeat enemies** to earn Calories and fund additional turrets
- **Eliminate the Boss** — the final wave ends with a Boss enemy that deals massive damage if it reaches your base

---

## 🌊 Enemy Waves

Enemies follow fixed waypoint paths toward your base. Each wave spawns more enemies with higher health, with a short delay between waves.

- Regular enemies deal **1 damage** to the base on arrival
- The **Boss** spawns as the last enemy of the final wave and deals massive damage
- Killing an enemy rewards **Calories**

---

## 🔫 Shooting

Switch to **Gun Mode** to engage enemies directly. Useful for finishing off low-health targets, but turrets are required to hold the line efficiently. Projectiles are destroyed on impact or after a short lifetime.

---

## 🏗️ Tower Placement

Switch to **Build Mode** to enter the placement interface. Buildable tiles across the map change color to show their state:

| Color | Meaning |
| --- | --- |
| 🟢 Green | Available to build on |
| 🟡 Yellow | Hovered |
| 🔵 Cyan | Selected |
| 🔴 Red | Occupied by a turret |

Click a green tile to open the **Shop UI** and choose a turret. Turrets you can afford are shown in **green**; those you cannot are shown in **red**. Placed turrets can be sold for a partial refund.

---

## 🗼 Turrets

Three turret types are available, each suited for a different defensive role:

| Turret | Description |
| --- | --- |
| **Machine Gun** | Fast, affordable, and reliable for holding choke points. Low damage per shot with a slight spread angle. |
| **Sniper** | Extreme range with high single-target damage and a slow fire rate. Has a chance to deal critical hits and uses a laser sight to track targets. |
| **Artillery** | Slow-firing but deals area damage on impact. Best placed where enemies cluster. |

> All turrets automatically acquire the nearest enemy in range, scan when idle, and track targets in full 3D.

---

## 🛠️ Built With

- [Unity](https://unity.com/) — game engine (Unity 6000.0.47f1)
- **C#** — game logic, turret AI, and wave management
- **Universal Render Pipeline (URP)** — rendering and visual effects

---

## 🚀 Running the Game

### Run in Unity Editor

1. Clone the repository:

   ```
   git clone https://github.com/mcheng1359/Viral-Defense.git
   ```

2. Open **Unity Hub** and click **Add project from disk**
3. Select the `Viral-Defense` folder
4. Open the project in Unity (**6000.0.47f1** recommended)
5. In the **Project** window, navigate to `Assets/Scenes/`
6. Open the main scene and press **Play**

---

## 📁 Project Structure

```
Viral-Defense/
├── Assets/              # Game scripts, scenes, prefabs, art, and audio
├── Packages/            # Unity package dependencies
├── ProjectSettings/     # Unity project configuration
├── WebGL Builds/        # Pre-built WebGL output for browser play
└── Final Project.slnx   # Visual Studio solution file
```

---

## 📄 License

This project is licensed under the [Apache 2.0 License](https://github.com/mcheng1359/Viral-Defense/blob/main/LICENSE).
