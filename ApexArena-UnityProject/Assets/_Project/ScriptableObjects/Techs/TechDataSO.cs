using System.Collections.Generic;
using UnityEngine;

namespace ApexArena.ScriptableObjects
{
    /// <summary>
    /// بيانات التقنية القابلة للإنشاء
    /// </summary>
    [CreateAssetMenu(fileName = "NewTech", menuName = "Apex Arena/Tech")]
    public class TechDataSO : ScriptableObject
    {
        public TechType TechType = TechType.AdaptiveArmor;
        public string TechName = "Adaptive Armor";
        [TextArea] public string Description = "Adapts to incoming damage type";
        public int CostEnergy = 0;
        public int CostData = 0;
        public int CostMaterials = 0;
        public float Cooldown = 0f;
        public float Duration = 0f;
        public List<TechType> Prerequisites = new List<TechType>();
        public Sprite TechIcon;
        public GameObject ActivationEffect;
        public AudioClip ActivationSound;
    }

    public enum TechType
    {
        AdaptiveArmor,
        Nanobots,
        QuantumLeap,
        WeaponOverdrive,
        Hologram
    }
}
