# Apex Arena — Unity Publishing Readiness

## Current status

The repository is a **Unity 2022.3.20f1** project with the Input System, URP, Netcode for GameObjects, Navigation, and related gameplay scripts committed. It also includes a GitHub Actions workflow that targets Windows and Android builds.

The project is **not yet ready to publish a playable build** because no Unity scene (`.unity`) or `ProjectSettings/EditorBuildSettings.asset` currently exists. Unity Cloud Build Automation and local Unity builds both require at least one configured scene.

## Required steps in Unity Editor

Open the project through Unity Hub using **Unity 2022.3.20f1** and complete the following once:

1. Create `Assets/_Project/Scenes/MainArena.unity`.
2. Add a camera, directional light, a ground collider, and a player GameObject with `CharacterController`, `ResourceManager`, and `PlayerController`.
3. Add a `GameManager` GameObject using `ApexArena.Core.GameManager`.
4. Save the scene and open **File → Build Settings**.
5. Select **Add Open Scenes** so `MainArena` is included in the build.
6. Choose a target platform such as Android for a phone build or Windows for desktop testing.
7. Run one local test build before enabling Unity Build Automation.

## Unity Cloud Build Automation

After signing into the Unity Dashboard, create or select an Organization and project, then connect the `abdelatizarzori3-sys/apex-arena-` repository. Configure a build target using Unity **2022.3.20f1** and select the `MainArena` scene from Build Settings.

For Android release builds, configure the app identifier, version, keystore, and signing passwords directly in Unity/Unity Cloud secrets. Do not commit keystores, passwords, tokens, or account credentials to this repository.

## GitHub Actions alternative

The existing `.github/workflows/build.yml` runs `game-ci/unity-builder` for Windows and Android. Before it can produce releases, add these repository secrets in GitHub:

| Secret | Purpose |
|---|---|
| `UNITY_LICENSE` | Unity activation license data |
| `UNITY_EMAIL` | Account email used by the build service |
| `UNITY_PASSWORD` | Password or compatible activation credential used only in GitHub Secrets |

Use Unity's current activation guidance for the selected license type. Never paste those values into source files, issues, messages, or workflow YAML.

## Verification checklist

- [ ] `MainArena.unity` exists under `Assets/_Project/Scenes/`.
- [ ] Build Settings contains `MainArena` as an enabled scene.
- [ ] The Unity Console has no C# compile errors.
- [ ] A local Windows or Android development build runs.
- [ ] Unity Cloud Build Automation or GitHub Actions reports a successful build.
- [ ] Android signing keys are configured in the appropriate secret store.

## What requires account access

Creating a Unity Cloud project, connecting the repository, uploading a build, configuring a license, creating an Android keystore, or publishing to a store requires a signed-in Unity account and explicit final confirmation before submission.
