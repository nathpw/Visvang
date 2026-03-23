# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Visvang** is a South African carp fishing chaos simulator built in Unity. Features authentic SA dam fishing with species-specific mechanics (barbel death rolls, mudfish slime), a pap & dip bait system, equipment progression, chaos events, QTEs, and multiplayer modes. The tone is humorous South African culture (Afrikaans slang, SA brand references).

## Build & Run

This is a **Unity project** (C#). Open in Unity Hub and let it compile.

- **Unity Version**: 2022.3 LTS or newer
- **Build**: Unity Editor > File > Build Settings (or Ctrl+Shift+B)
- **Play**: Press Play in Unity Editor to test
- **Scripts compile**: Automatically on save when Unity Editor is open
- **Scene setup**: Create an empty scene, add one empty GameObject, attach `GameBootstrap` component. Press Play. Everything else is created automatically.

## Architecture

### Bootstrap Flow (GameBootstrap.cs)
1. `Awake()`: Creates all manager singletons (GameManager, FishDatabase, FishSpawner, BaitManager, PapSystem, EquipmentManager, ChaosEventManager, QTEManager, XPSystem, PlayerProfile, AudioManager) + wires FishingController with sub-systems via reflection
2. `Start()`: Loads all data via `RuntimeDataLoader`, builds 3D dam environment via `DamEnvironmentBuilder`, builds entire UI via `UIBuilder`, creates `GameFlow` master controller

### Core Pattern
- **Singleton MonoBehaviours**: All managers use `Instance` pattern
- **ScriptableObjects for data**: Fish, dips, bait, rods, reels are `ScriptableObject` subclasses
- **Runtime data loading**: `FishDataFactory`, `DipDataFactory`, `RodDataFactory` create all game data at startup; for production, migrate to `.asset` files
- **Programmatic UI**: `UIBuilder` creates the entire Canvas and all panels from code (no scene file dependency)
- **Programmatic environment**: `DamEnvironmentBuilder` creates camera, lights, water, terrain, scenery from code
- **Event-driven communication**: Systems communicate via C# `event Action<T>` delegates
- **State machine**: `FishingController` uses `FishingState` enum; `GameManager` uses `GamePhase` enum

### Namespace Structure (43 scripts)
```
Visvang.Core        → GameManager, GameState enums, Constants, GameBootstrap, GameFlow (master controller),
                      DamEnvironmentBuilder, InputController, CompatHelper
Visvang.Save        → SaveManager (JSON file I/O), SaveData (full save model), SaveBridge (manager↔save sync),
                      SessionTracker (per-session stats, auto-save on catch/end)
Visvang.Fish        → FishData (SO), FishDatabase, FishBehaviour (species AI), FishSpawner
Visvang.Fishing     → FishingController (state machine), FightController, TensionSystem, CastingSystem
Visvang.Bait        → BaitData (SO), DipData (SO), PapSystem (bucket), BaitManager
Visvang.Equipment   → RodData (SO), ReelData (SO), EquipmentManager, UpgradeSystem
Visvang.Events      → ChaosEventManager (14 event types, dam hazards)
Visvang.QTE         → QTEManager (tap, hold, grip strength, slime wash)
Visvang.Progression → XPSystem, PlayerProfile (save/load via PlayerPrefs)
Visvang.UI          → UIBuilder (programmatic), UIReferences, UIManager, HUDController,
                      MessageSystem (SA humor strings), FishCaughtPanel
Visvang.Multiplayer → MultiplayerManager (Barber Battle, Mudfish Madness, Carp Only League)
Visvang.Audio       → AudioManager (SFX, music, ambience)
Visvang.Data        → FishDataFactory (16 species), DipDataFactory (11 dips),
                      RodDataFactory (10 rods, 3 reels), RuntimeDataLoader
```

### Game Loop Flow
```
MainMenu → GearSetup (select dip) → Fishing:
  Idle → Casting (hold/release) → Waiting → BiteDetected → Strike → Fighting → Landing → Caught/Lost
                                                                       ↕
                                                                 RodPulledIn (barbel QTE)
```

### Key System Interactions
- **GameFlow** is the master controller: wires all UI buttons, handles all input, drives screen transitions, subscribes to all system events
- **FishSpawner** rolls which fish bites based on: active dip, time of day, weather, depth → fires `OnFishBite`
- **FishingController** receives bite, manages hook set, creates `FishBehaviour` for the fight
- **FishBehaviour** runs species-specific AI: barbel has death rolls + tail slaps, mudfish causes slime + tangles
- **TensionSystem** tracks line tension during fights — too high = snap, too low = escape
- **FightController** manages reel progress, QTE triggers, and fight resolution
- **ChaosEventManager** periodically rolls random events (barbel ambush at night, mudfish swarms, bird theft)
- **PapSystem** tracks bait bucket level — barbel destroys pap, mudfish ruins quality, each cast consumes some; empty bucket = session over
- **EquipmentManager** provides stat multipliers (reel speed, cast accuracy, line strength, grip, barbel resistance)
- **PlayerProfile** persists XP, level, catch history, records via PlayerPrefs

### Species-Specific Mechanics
- **Barbel**: violent taker, death roll QTE (grip strength mash), tail slap disorients UI for 3s, can pull rod into water, destroys pap
- **Mudfish**: slime meter reduces reel control (press W to wash), frequent tangles, high hook shake chance, 3-streak triggers pap bucket penalty
- **Carp**: standard fight, slow/heavy takers, bread and butter
- **Legendary fish**: 10x XP multiplier, extremely rare spawn conditions (Boknes Golden Carp, Flat-nose River Barber)

### Dip System (affects which species bite)
- `BarbelAttractor`: Garlic, Bloodworm, Devil's Fork — high risk/reward, attracts massive barbel
- `MudfishAttractor`: Cheap Sweetcorn, Vanilla, Banjo, Pink Sweets — good for beginners but mudfish spam
- `MudfishRepellent`: Black Magic, Synthetic Attractor — premium, reduces mudfish chance
- `CarpSpecialist`: Competition Premium — expensive, targeted carp fishing

### Equipment Tiers
- **Entry**: Makro Plastic Special (R49.99), Bent Steel Rod, Budget Fibreglass (snaps on barbel!)
- **Mid**: Sensation Carp Rod, Okuma Tournament, Daiwa Long Cast, Shimano Aerlex
- **High**: Barbel Specialist Power Rod, Competition European Carp Gear
- **Legendary**: Oom Frik's Handcrafted Monster Rod (level 40 unlock)

## Conventions

- All game constants in `Constants.cs` — never hardcode magic numbers
- SA humor messages in `GameFlow.cs` `ShowCatchMessage()` and `MessageSystem.cs` — per-species string arrays
- GameBootstrap wires FishingController's private serialized fields via reflection (`SetPrivateField`)
- Player input handled in `GameFlow.Update()` — supports both touch (mobile) and mouse click (desktop)
- All UI created programmatically by `UIBuilder.Build()` — no scene file dependencies
- Dip selection happens in GearSetup phase via dynamically created buttons from `BaitManager.AllDips`

## Save System

Full JSON-based save system at `Application.persistentDataPath/visvang_save.json`.

### Save Architecture
- **SaveData** — single serializable class holding ALL game state (player, equipment, inventory, progress, statistics, session history, catch log)
- **SaveManager** — reads/writes JSON to disk, auto-saves every 60s when dirty, saves on app pause/quit, backup file rotation
- **SaveBridge** — syncs between runtime managers and SaveData (restore on load, collect on save)
- **SessionTracker** — tracks per-session stats (catches, XP, duration, chaos events), writes SessionRecord on session end

### Save Flow
1. **Boot**: `GameBootstrap.Start()` → `SaveManager.Load()` → `SaveBridge.RestoreFromSave()` → all managers populated from save
2. **During play**: `PlayerProfile.RecordCatch()` and `SessionTracker.LogCatch()` mark save dirty → auto-save fires every 60s
3. **Session end**: `SessionTracker.EndSession()` writes SessionRecord → `SaveBridge.CollectToSave()` → `SaveManager.Save()` writes to disk
4. **App quit/pause**: `SaveManager.OnApplicationQuit/Pause` flushes any pending dirty data

### What's Persisted
- Player level, XP, currency, name
- All caught species (discovery tracking)
- Equipment owned (rods, reels by name), equipped loadout, accessories
- Dip/bait inventory quantities
- Unlocked upgrades
- Full statistics (catches, losses, records, slaps, rods lost, casts, play time)
- Complete catch log with timestamps, dip/rod used, session index
- Session history with per-session breakdown

### Equipment/Dip Name Resolution
Equipment and dips are saved by **name string** and resolved back to runtime ScriptableObjects via `SaveBridge` name lookups against the factories. If a name isn't found (e.g., removed item), it's silently skipped.
