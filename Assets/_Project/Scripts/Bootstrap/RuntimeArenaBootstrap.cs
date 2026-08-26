using ApexArena.Core;
using ApexArena.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ApexArena.Bootstrap
{
    /// <summary>
    /// Creates the minimum playable arena when MainArena loads. This keeps the
    /// initial build self-contained while art, multiplayer, and UI prefabs are
    /// developed in subsequent production passes.
    /// </summary>
    public static class RuntimeArenaBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateArena()
        {
            if (GameObject.Find("Apex Arena Runtime") != null)
            {
                return;
            }

            var runtimeRoot = new GameObject("Apex Arena Runtime");
            CreateCamera(runtimeRoot.transform);
            CreateGround(runtimeRoot.transform);
            CreateMatchManager(runtimeRoot.transform);
            CreatePlayer(runtimeRoot.transform);
            CreateLight(runtimeRoot.transform);
        }

        private static void CreateCamera(Transform parent)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<ArenaCameraFollow>();
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena Ground";
            ground.transform.SetParent(parent);
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            SetColor(ground, new Color(0.08f, 0.14f, 0.17f));

            var spawnPad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spawnPad.name = "Spawn Pad";
            spawnPad.transform.SetParent(parent);
            spawnPad.transform.position = new Vector3(0f, 0.05f, 0f);
            spawnPad.transform.localScale = new Vector3(2f, 0.1f, 2f);
            SetColor(spawnPad, new Color(0.08f, 0.8f, 0.8f));
        }

        private static void CreateMatchManager(Transform parent)
        {
            var matchManager = new GameObject("Match Manager");
            matchManager.transform.SetParent(parent);
            matchManager.AddComponent<GameManager>();
        }

        private static void CreatePlayer(Transform parent)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Operative";
            player.transform.SetParent(parent);
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.Destroy(player.GetComponent<Collider>());
            player.AddComponent<CharacterController>();
            player.AddComponent<ResourceManager>();
            var controller = player.AddComponent<PlayerController>();
            controller.PlayerName = "Operative";
            player.AddComponent<ArenaKeyboardInput>();
            player.AddComponent<ArenaMobileInput>();
            SetColor(player, new Color(0.95f, 0.38f, 0.18f));
        }

        private static void CreateLight(Transform parent)
        {
            var lightObject = new GameObject("Arena Directional Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var arenaLight = lightObject.AddComponent<Light>();
            arenaLight.type = LightType.Directional;
            arenaLight.intensity = 1.2f;
            arenaLight.color = new Color(0.82f, 0.92f, 1f);
        }

        private static void SetColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }

    public sealed class ArenaCameraFollow : MonoBehaviour
    {
        private Transform target;
        private readonly Vector3 offset = new Vector3(0f, 7.5f, -8f);

        private void LateUpdate()
        {
            if (target == null)
            {
                var player = Object.FindFirstObjectByType<PlayerController>();
                target = player != null ? player.transform : null;
            }

            if (target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, target.position + offset, 8f * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.1f);
        }
    }

    public sealed class ArenaKeyboardInput : MonoBehaviour
    {
        private PlayerController player;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (player == null || Keyboard.current == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            var move = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f)
            );

            player.SetMoveInput(move.normalized);
            player.SetRunInput(keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                player.OnJump();
            }
        }
    }
}
