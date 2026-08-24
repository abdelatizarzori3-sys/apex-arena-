using System.Collections.Generic;
using UnityEngine;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// نظام التقنيات - يدير فتح وتفعيل التقنيات
    /// Tech System - manages unlocking and activating technologies
    /// </summary>
    public class TechSystem : MonoBehaviour
    {
        [Header("Techs")]
        [SerializeField] private List<TechData> availableTechs = new List<TechData>();
        [SerializeField] private List<TechData> unlockedTechs = new List<TechData>();
        [SerializeField] private TechData activeTech;

        [Header("Player Reference")]
        [SerializeField] private PlayerController player;
        [SerializeField] private ResourceManager resourceManager;

        private Dictionary<TechType, TechData> techMap = new Dictionary<TechType, TechData>();
        private Dictionary<TechType, float> techCooldowns = new Dictionary<TechType, float>();

        public event System.Action<TechData> OnTechUnlocked;
        public event System.Action<TechData> OnTechActivated;
        public event System.Action<TechData> OnTechDeactivated;

        private void Awake()
        {
            if (player == null) player = GetComponent<PlayerController>();
            if (resourceManager == null) resourceManager = GetComponent<ResourceManager>();

            InitializeTechs();
        }

        private void Update()
        {
            UpdateCooldowns();
            UpdateActiveTech();
        }

        private void InitializeTechs()
        {
            techMap.Clear();
            foreach (var tech in availableTechs)
            {
                techMap[tech.TechType] = tech;
                techCooldowns[tech.TechType] = 0f;
            }
        }

        /// <summary>
        /// فتح تقنية جديدة
        /// </summary>
        public bool UnlockTech(TechType techType)
        {
            if (!techMap.TryGetValue(techType, out var tech)) return false;
            if (unlockedTechs.Contains(tech)) return false;

            // التحقق من المتطلبات
            if (!CheckPrerequisites(tech)) return false;

            // التحقق من التكلفة
            if (!resourceManager.SpendResources(tech.CostEnergy, tech.CostData, tech.CostMaterials)) 
                return false;

            unlockedTechs.Add(tech);
            OnTechUnlocked?.Invoke(tech);

            Debug.Log($"[TechSystem] Unlocked: {tech.TechName}");
            return true;
        }

        /// <summary>
        /// تفعيل تقنية
        /// </summary>
        public bool ActivateTech(TechType techType)
        {
            if (!techMap.TryGetValue(techType, out var tech)) return false;
            if (!unlockedTechs.Contains(tech)) return false;
            if (techCooldowns[techType] > 0) return false;

            // إلغاء التقنية النشطة السابقة
            if (activeTech != null)
            {
                DeactivateCurrentTech();
            }

            activeTech = tech;
            techCooldowns[techType] = tech.Cooldown;

            ApplyTechEffects(tech);
            OnTechActivated?.Invoke(tech);

            Debug.Log($"[TechSystem] Activated: {tech.TechName}");
            return true;
        }

        public void ActivateCurrentTech()
        {
            if (activeTech != null)
            {
                ActivateTech(activeTech.TechType);
            }
        }

        public void DeactivateCurrentTech()
        {
            if (activeTech == null) return;

            RemoveTechEffects(activeTech);
            OnTechDeactivated?.Invoke(activeTech);

            Debug.Log($"[TechSystem] Deactivated: {activeTech.TechName}");
            activeTech = null;
        }

        public bool IsTechActive(TechType techType)
        {
            return activeTech != null && activeTech.TechType == techType;
        }

        public bool IsTechUnlocked(TechType techType)
        {
            return techMap.TryGetValue(techType, out var tech) && unlockedTechs.Contains(tech);
        }

        private bool CheckPrerequisites(TechData tech)
        {
            foreach (var prereq in tech.Prerequisites)
            {
                if (!IsTechUnlocked(prereq)) return false;
            }
            return true;
        }

        private void ApplyTechEffects(TechData tech)
        {
            switch (tech.TechType)
            {
                case TechType.AdaptiveArmor:
                    // تطبيق في PlayerController.TakeDamage
                    break;
                case TechType.Nanobots:
                    StartCoroutine(NanobotRepair());
                    break;
                case TechType.QuantumLeap:
                    // تفعيل القفزة
                    break;
                case TechType.WeaponOverdrive:
                    // تفعيل في WeaponSystem
                    break;
                case TechType.Hologram:
                    SpawnHologram();
                    break;
            }
        }

        private void RemoveTechEffects(TechData tech)
        {
            switch (tech.TechType)
            {
                case TechType.Nanobots:
                    StopCoroutine(NanobotRepair());
                    break;
                case TechType.Hologram:
                    DestroyHologram();
                    break;
            }
        }

        private System.Collections.IEnumerator NanobotRepair()
        {
            while (activeTech?.TechType == TechType.Nanobots)
            {
                yield return new WaitForSeconds(1f);
                player?.Heal(5);
            }
        }

        private void SpawnHologram()
        {
            // TODO: Instantiate hologram prefab
            Debug.Log("[TechSystem] Hologram spawned");
        }

        private void DestroyHologram()
        {
            // TODO: Destroy hologram
        }

        private void UpdateCooldowns()
        {
            var keys = new List<TechType>(techCooldowns.Keys);
            foreach (var key in keys)
            {
                if (techCooldowns[key] > 0)
                {
                    techCooldowns[key] -= Time.deltaTime;
                }
            }
        }

        private void UpdateActiveTech()
        {
            if (activeTech != null && activeTech.Duration > 0)
            {
                // TODO: Check duration and deactivate
            }
        }
    }

    public enum TechType
    {
        AdaptiveArmor,
        Nanobots,
        QuantumLeap,
        WeaponOverdrive,
        Hologram
    }

    [System.Serializable]
    public class TechData
    {
        public TechType TechType;
        public string TechName;
        public string Description;
        public int CostEnergy;
        public int CostData;
        public int CostMaterials;
        public float Cooldown;
        public float Duration;
        public List<TechType> Prerequisites = new List<TechType>();
        public Sprite TechIcon;
    }
}
