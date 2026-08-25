# Apex Arena — Desktop Unity Verification

This checklist is the final local gate before starting the Android Build Automation job. It must be completed on a computer; the Unity dashboard on a phone cannot import the project or verify C# compilation.

## 1. Open the synchronized source

Clone the repository or pull the latest `main` branch, then open the project root in Unity Hub. The required editor version is **Unity 2022.3.20f1** with **Android Build Support** installed, including the SDK, NDK, and OpenJDK components.

```text
https://github.com/abdelatizarzori3-sys/apex-arena-
```

When Unity imports the project, allow Package Manager to resolve all dependencies. Netcode for GameObjects 1.6.0 resolves Unity Transport 1.3.4 as a dependency, which supplies the `UnityTransport` type used by the network wrapper.

## 2. Verify the scene and Console

Open `Assets/_Project/Scenes/MainArena.unity`. The scene is intentionally lightweight; it starts an arena bootstrap that creates the camera, ground, light, match manager, and player at runtime. Press **Play** and confirm the Console has no compilation errors. The player should move with **WASD**, run with **Shift**, and jump with **Space**.

> Stop here and fix any red Console entry before continuing. A successful compile and Play session is the minimum validation needed before a cloud artifact is trusted.

## 3. Verify Android Build Settings

Open **File → Build Settings**. The only enabled scene must be `Assets/_Project/Scenes/MainArena.unity`. Select **Android**, use **Switch Platform** if necessary, then open **Player Settings** and set a unique application identifier such as `com.abdelatizarzori.apexarena`.

| Item | Required value |
|---|---|
| Unity editor | `2022.3.20f1` |
| Active platform | Android |
| Enabled scene | `Assets/_Project/Scenes/MainArena.unity` |
| Development Build | Off for the first distributable APK |
| Application identifier | A unique reverse-domain identifier owned by the publisher |

## 4. Cloud Build configuration

In Unity Cloud on the computer, open **Apex Arena → Build Automation**. In the Build settings source-control area, connect GitHub and authorize access to `abdelatizarzori3-sys/apex-arena-`; select branch `main`. Then open **Configurations**, select **New configuration**, choose **Android**, and set Unity to `2022.3.20f1`. Confirm that the target uses the build settings scene `Assets/_Project/Scenes/MainArena.unity`.

Review the displayed organization plan, credits, and build cost before triggering the first job. Run it as a private verification build, download the APK, and install it on a test Android device before any store submission.

## 5. Evidence to retain

Keep a screenshot of the clean Console, the Build Settings scene list, the successful cloud build status, and the device launch. These four checks are the evidence that distinguishes a prepared Unity source tree from a genuinely build-verified Android application.
