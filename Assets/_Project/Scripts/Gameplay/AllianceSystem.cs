using System.Collections.Generic;
using UnityEngine;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// نظام التحالفات - يدير العلاقات بين اللاعبين
    /// Alliance System - manages relationships between players
    /// </summary>
    public class AllianceSystem : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController owner;

        [Header("Alliances")]
        [SerializeField] private List<PlayerController> allies = new List<PlayerController>();
        [SerializeField] private List<PlayerController> enemies = new List<PlayerController>();
        [SerializeField] private int maxAllianceSize = 4;

        [Header("Reputation")]
        [SerializeField] private int betrayalPenalty = -15;
        [SerializeField] private int allianceBonus = 5;

        public IReadOnlyList<PlayerController> Allies => allies;
        public IReadOnlyList<PlayerController> Enemies => enemies;

        public event System.Action<PlayerController, AllianceAction> OnAllianceChanged;
        public event System.Action<PlayerController> OnBetrayal;

        private void Awake()
        {
            if (owner == null) owner = GetComponent<PlayerController>();
        }

        /// <summary>
        /// عرض تحالف
        /// </summary>
        public bool OfferAlliance(PlayerController target)
        {
            if (target == null || target == owner) return false;
            if (allies.Count >= maxAllianceSize) return false;
            if (allies.Contains(target) || enemies.Contains(target)) return false;

            // إرسال العرض
            var targetAlliance = target.GetComponent<AllianceSystem>();
            if (targetAlliance != null)
            {
                targetAlliance.ReceiveAllianceOffer(owner);
                return true;
            }
            return false;
        }

        /// <summary>
        /// استقبال عرض تحالف
        /// </summary>
        public void ReceiveAllianceOffer(PlayerController fromPlayer)
        {
            // TODO: Show UI prompt to player
            Debug.Log($"[AllianceSystem] {fromPlayer.PlayerName} offered alliance to {owner.PlayerName}");
        }

        /// <summary>
        /// قبول تحالف
        /// </summary>
        public bool AcceptAlliance(PlayerController fromPlayer)
        {
            if (fromPlayer == null) return false;
            if (allies.Contains(fromPlayer)) return false;

            allies.Add(fromPlayer);

            // إضافة متبادلة
            var fromAlliance = fromPlayer.GetComponent<AllianceSystem>();
            if (fromAlliance != null && !fromAlliance.allies.Contains(owner))
            {
                fromAlliance.allies.Add(owner);
            }

            // مكافأة السمعة
            owner.GetComponent<ResourceManager>()?.AddResource(ResourceType.Reputation, allianceBonus);

            OnAllianceChanged?.Invoke(fromPlayer, AllianceAction.Formed);
            Debug.Log($"[AllianceSystem] Alliance formed between {owner.PlayerName} and {fromPlayer.PlayerName}");
            return true;
        }

        /// <summary>
        /// خيانة تحالف
        /// </summary>
        public bool BetrayAlliance(PlayerController target)
        {
            if (target == null || !allies.Contains(target)) return false;

            // إزالة من التحالف
            allies.Remove(target);
            enemies.Add(target);

            // إزالة متبادلة
            var targetAlliance = target.GetComponent<AllianceSystem>();
            if (targetAlliance != null)
            {
                targetAlliance.allies.Remove(owner);
                targetAlliance.enemies.Add(owner);
            }

            // عقوبة السمعة
            owner.GetComponent<ResourceManager>()?.AddResource(ResourceType.Reputation, betrayalPenalty);

            OnBetrayal?.Invoke(target);
            OnAllianceChanged?.Invoke(target, AllianceAction.Betrayed);

            Debug.Log($"[AllianceSystem] {owner.PlayerName} betrayed {target.PlayerName}!");
            return true;
        }

        /// <summary>
        /// إعلان العداء
        /// </summary>
        public void DeclareEnemy(PlayerController target)
        {
            if (target == null || enemies.Contains(target)) return;

            enemies.Add(target);
            OnAllianceChanged?.Invoke(target, AllianceAction.EnemyDeclared);
        }

        /// <summary>
        /// إعلان الهدنة
        /// </summary>
        public void DeclareTruce(PlayerController target)
        {
            if (target == null) return;

            enemies.Remove(target);
            OnAllianceChanged?.Invoke(target, AllianceAction.Truce);
        }

        public AllianceStatus GetRelationship(PlayerController player)
        {
            if (allies.Contains(player)) return AllianceStatus.Ally;
            if (enemies.Contains(player)) return AllianceStatus.Enemy;
            return AllianceStatus.Neutral;
        }

        public bool IsAlly(PlayerController player)
        {
            return allies.Contains(player);
        }

        public bool IsEnemy(PlayerController player)
        {
            return enemies.Contains(player);
        }
    }

    public enum AllianceAction
    {
        Formed,
        Betrayed,
        EnemyDeclared,
        Truce,
        Broken
    }

    public enum AllianceStatus
    {
        Ally,
        Neutral,
        Enemy
    }
}
