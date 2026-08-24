using UnityEngine;

namespace ApexArena.ScriptableObjects
{
    /// <summary>
    /// بيانات السلاح القابلة للإنشاء
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Apex Arena/Weapon")]
    public class WeaponDataSO : ScriptableObject
    {
        public string WeaponName = "Plasma Rifle";
        public WeaponType Type = WeaponType.PlasmaRifle;
        public int Damage = 35;
        public float FireRate = 600f; // RPM
        public int MagazineSize = 30;
        public int MaxAmmo = 300;
        public float EffectiveRange = 150f;
        public float MaxRange = 300f;
        public float Spread = 0.05f;
        public float ReloadTime = 2f;
        public float HeatPerShot = 5f;
        public float MaxHeat = 100f;
        public float CooldownRate = 20f;
        public float CooldownTime = 3f;
        public DamageType DamageType = DamageType.Energy;
        public Sprite WeaponIcon;
        public GameObject WeaponPrefab;
        public AudioClip FireSound;
        public AudioClip ReloadSound;
    }

    public enum WeaponType
    {
        PlasmaRifle,
        ThermalPistol,
        ParticleSMG,
        QuantumSniper,
        HackPistol,
        NanobotLauncher
    }

    public enum DamageType
    {
        Physical,
        Energy,
        Thermal,
        Radiation,
        Quantum
    }
}
