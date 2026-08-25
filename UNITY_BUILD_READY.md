# Apex Arena — Build-Ready Baseline

## Current delivery state

The project now has a committed build entry point at `Assets/_Project/Scenes/MainArena.unity` and that scene is registered in `ProjectSettings/EditorBuildSettings.asset`. `MainArena` is intentionally minimal: it is an empty authored scene that triggers the runtime arena bootstrap after loading.

At runtime, `RuntimeArenaBootstrap` creates the camera, directional light, arena floor, spawn pad, match manager, and a controllable player. Keyboard controls are **WASD** for movement, **Shift** to run, and **Space** to jump. The bootstrap exists to make the project buildable and demonstrable while production art, UI, audio, AI, combat prefabs, and real multiplayer infrastructure are completed separately.

## Build target

Configure Unity Build Automation with these values:

| Setting | Value |
|---|---|
| Repository | `abdelatizarzori3-sys/apex-arena-` |
| Branch | `main` |
| Unity version | `2022.3.20f1` |
| Build target | Android first; Windows is optional for desktop verification |
| Scene | `Assets/_Project/Scenes/MainArena.unity` |
| Output | APK for device verification, then AAB only when preparing a Play Store release |

## Source-level fixes included

The match manager now permits a solo verification session to remain active instead of ending immediately. The player controller exposes small movement setters for the bootstrap input adapter. The network wrapper now uses `UnityTransport` from `Unity.Netcode.Transports.UTP` instead of the removed UNet transport reference, matching Netcode for GameObjects 1.6.

## Validation boundary

The sandbox does not contain Unity Editor, so it cannot execute a local Unity compilation or device build. The committed changes have been checked for whitespace errors, the scene GUID matches the Build Settings entry, and deprecated UNet references were removed. Build Automation is the next runtime validation step and may report Unity-specific import or package errors that should be resolved before distributing the artifact publicly.

## Publication guardrail

Run the first cloud build as a private verification artifact. Do not upload to an app store or make a public release until the build succeeds and the APK has been tested on a device.
