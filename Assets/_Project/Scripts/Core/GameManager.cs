using System;
using System.Collections.Generic;
using UnityEngine;

namespace ApexArena.Core
{
    /// <summary>
    /// مدير اللعبة الرئيسي - Singleton يتحكم في حالة المباراة
    /// Game Manager - Main singleton controlling match state
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private int maxPlayers = 100;
        [SerializeField] private float matchDuration = 1800f; // 30 دقيقة
        [SerializeField] private string algorithmVersion = "2.4.1";

        [Header("Zone Settings")]
        [SerializeField] private float zoneMutationInterval = 180f; // 3 دقائق
        [SerializeField] private float dangerZoneExpandRate = 0.15f; // 15%

        public MatchState CurrentMatchState { get; private set; }
        public float ElapsedTime { get; private set; }
        public int AlivePlayers { get; private set; }
        public int CurrentPlayerCount { get; private set; }

        public event System.Action<MatchState> OnMatchStateChanged;
        public event System.Action<float> OnMatchTimeUpdated;
        public event System.Action OnZoneMutation;

        private List<PlayerController> activePlayers = new List<PlayerController>();
        private float nextMutationTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeMatch();
        }

        private void Update()
        {
            if (CurrentMatchState != MatchState.Active) return;

            ElapsedTime += Time.deltaTime;
            OnMatchTimeUpdated?.Invoke(ElapsedTime);

            // التحقق من التغييرات البيئية
            if (Time.time >= nextMutationTime)
            {
                TriggerZoneMutation();
                nextMutationTime = Time.time + zoneMutationInterval;
            }

            // التحقق من انتهاء المباراة
            if (ElapsedTime >= matchDuration || AlivePlayers <= 1)
            {
                EndMatch();
            }
        }

        public void InitializeMatch()
        {
            CurrentMatchState = MatchState.Lobby;
            ElapsedTime = 0f;
            AlivePlayers = 0;
            CurrentPlayerCount = 0;
            nextMutationTime = Time.time + zoneMutationInterval;

            Debug.Log($"[GameManager] Match initialized - Algorithm v{algorithmVersion}");
        }

        public void StartMatch()
        {
            CurrentMatchState = MatchState.Active;
            OnMatchStateChanged?.Invoke(CurrentMatchState);

            Debug.Log($"[GameManager] Match started with {CurrentPlayerCount} players");
        }

        public void EndMatch()
        {
            CurrentMatchState = MatchState.Ended;
            OnMatchStateChanged?.Invoke(CurrentMatchState);

            Debug.Log($"[GameManager] Match ended. Winner: {GetWinner()?.PlayerName ?? "None"}");
        }

        public void RegisterPlayer(PlayerController player)
        {
            if (!activePlayers.Contains(player))
            {
                activePlayers.Add(player);
                CurrentPlayerCount = activePlayers.Count;
                AlivePlayers++;
            }
        }

        public void UnregisterPlayer(PlayerController player)
        {
            if (activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                CurrentPlayerCount = activePlayers.Count;
                if (player.IsAlive)
                {
                    AlivePlayers--;
                }
            }
        }

        public void OnPlayerDeath(PlayerController player)
        {
            AlivePlayers--;
            Debug.Log($"[GameManager] {player.PlayerName} eliminated. {AlivePlayers} players remaining.");
        }

        private void TriggerZoneMutation()
        {
            OnZoneMutation?.Invoke();
            DynamicEnvironment.Instance?.MutateZones();
            Debug.Log("[GameManager] Zone mutation triggered!");
        }

        private PlayerController GetWinner()
        {
            if (AlivePlayers == 1)
            {
                return activePlayers.Find(p => p.IsAlive);
            }
            return null;
        }
    }

    public enum MatchState
    {
        Lobby,
        Warmup,
        Active,
        Ended,
        Cancelled
    }
}
