using UnityEngine;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// مدير الموارد - يدير الطاقة، البيانات، والمواد
    /// Resource Manager - manages energy, data, and materials
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private int energy = 850;
        [SerializeField] private int data = 1200;
        [SerializeField] private int materials = 430;
        [SerializeField] private int reputation = 72;

        [Header("Limits")]
        [SerializeField] private int maxEnergy = 1000;
        [SerializeField] private int maxData = 5000;
        [SerializeField] private int maxMaterials = 3000;
        [SerializeField] private int maxReputation = 100;

        public int Energy => energy;
        public int Data => data;
        public int Materials => materials;
        public int Reputation => reputation;

        public event System.Action<ResourceType, int> OnResourceChanged;
        public event System.Action OnResourcesDepleted;

        public void AddResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Energy:
                    energy = Mathf.Min(energy + amount, maxEnergy);
                    break;
                case ResourceType.Data:
                    data = Mathf.Min(data + amount, maxData);
                    break;
                case ResourceType.Materials:
                    materials = Mathf.Min(materials + amount, maxMaterials);
                    break;
                case ResourceType.Reputation:
                    reputation = Mathf.Min(reputation + amount, maxReputation);
                    break;
            }
            OnResourceChanged?.Invoke(type, GetResourceAmount(type));
        }

        public bool SpendResources(int energyCost, int dataCost, int materialsCost)
        {
            if (energy < energyCost || data < dataCost || materials < materialsCost)
                return false;

            energy -= energyCost;
            data -= dataCost;
            materials -= materialsCost;

            OnResourceChanged?.Invoke(ResourceType.Energy, energy);
            OnResourceChanged?.Invoke(ResourceType.Data, data);
            OnResourceChanged?.Invoke(ResourceType.Materials, materials);

            if (energy <= 0 && data <= 0 && materials <= 0)
            {
                OnResourcesDepleted?.Invoke();
            }

            return true;
        }

        public bool HasEnoughResources(int energyCost, int dataCost, int materialsCost)
        {
            return energy >= energyCost && data >= dataCost && materials >= materialsCost;
        }

        public int GetResourceAmount(ResourceType type)
        {
            return type switch
            {
                ResourceType.Energy => energy,
                ResourceType.Data => data,
                ResourceType.Materials => materials,
                ResourceType.Reputation => reputation,
                _ => 0
            };
        }

        public float GetResourcePercentage(ResourceType type)
        {
            return type switch
            {
                ResourceType.Energy => (float)energy / maxEnergy,
                ResourceType.Data => (float)data / maxData,
                ResourceType.Materials => (float)materials / maxMaterials,
                ResourceType.Reputation => (float)reputation / maxReputation,
                _ => 0f
            };
        }
    }

    public enum ResourceType
    {
        Energy,
        Data,
        Materials,
        Reputation
    }
}
