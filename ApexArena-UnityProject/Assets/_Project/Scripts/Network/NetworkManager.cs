using Unity.Netcode;
using UnityEngine;
using System;

namespace ApexArena.Network
{
    /// <summary>
    /// مدير الشبكة - يدير الاتصال متعدد اللاعبين
    /// Network Manager - manages multiplayer connection
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Network Settings")]
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private int maxPlayers = 100;

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;

        private Unity.Netcode.NetworkManager netManager;

        public bool IsHost => netManager?.IsHost ?? false;
        public bool IsClient => netManager?.IsClient ?? false;
        public bool IsServer => netManager?.IsServer ?? false;

        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            netManager = Unity.Netcode.NetworkManager.Singleton;
            if (netManager != null)
            {
                netManager.OnClientConnectedCallback += HandleClientConnected;
                netManager.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        public void StartHost()
        {
            if (netManager == null) return;

            netManager.StartHost();
            Debug.Log($"[NetworkManager] Host started on port {serverPort}");
        }

        public void StartClient()
        {
            if (netManager == null) return;

            var transport = netManager.GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>();
            if (transport != null)
            {
                transport.ConnectAddress = serverAddress;
                transport.ConnectPort = (ushort)serverPort;
            }

            netManager.StartClient();
            Debug.Log($"[NetworkManager] Connecting to {serverAddress}:{serverPort}");
        }

        public void Disconnect()
        {
            if (netManager == null) return;

            if (IsHost)
            {
                netManager.Shutdown();
            }
            else if (IsClient)
            {
                netManager.Shutdown();
            }

            OnClientDisconnected?.Invoke();
            Debug.Log("[NetworkManager] Disconnected");
        }

        private void HandleClientConnected(ulong clientId)
        {
            OnClientConnected?.Invoke();
            OnPlayerJoined?.Invoke(clientId);

            Debug.Log($"[NetworkManager] Client {clientId} connected");
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            OnClientDisconnected?.Invoke();
            OnPlayerLeft?.Invoke(clientId);

            Debug.Log($"[NetworkManager] Client {clientId} disconnected");
        }

        /// <summary>
        /// إرسال حالة اللاعب للخادم
        /// </summary>
        public void SendPlayerState(Vector3 position, Vector3 rotation, int health)
        {
            if (!IsClient) return;

            // TODO: Implement with NetworkVariables or RPCs
        }

        /// <summary>
        /// مزامنة حالة اللعبة
        /// </summary>
        public void SyncGameState()
        {
            if (!IsServer) return;

            // TODO: Broadcast game state to all clients
        }

        private void OnDestroy()
        {
            if (netManager != null)
            {
                netManager.OnClientConnectedCallback -= HandleClientConnected;
                netManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
    }
}
