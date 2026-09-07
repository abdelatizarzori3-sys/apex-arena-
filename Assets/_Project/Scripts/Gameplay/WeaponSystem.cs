using UnityEngine;
using System.Collections.Generic;

namespace ApexArena.Gameplay
{
    public class WeaponSystem : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private List<WeaponData> availableWeapons = new List<WeaponData>();
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private Transform firePoint;
        [Header("Ammo")]
        [SerializeField] private int currentAmmo;
        [SerializeField] private int reserveAmmo;
        [Header("State")]
        [SerializeField] private bool isReloading;
        [SerializeField] private bool isOverheated;
        [SerializeField] private float overheatLevel;
        [Header("VFX")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private ParticleSystem shellEjection;
        private float lastFireTime;
        private float reloadTimer;
        public WeaponData CurrentWeapon => currentWeapon;
        public int CurrentAmmo => currentAmmo;
        public bool IsReloading => isReloading;
        public event System.Action OnWeaponFired;
        public event System.Action OnWeaponReloaded;
        public event System.Action OnWeaponSwitched;
        private void Start()
        {
            if (currentWeapon != null) EquipWeapon(currentWeapon);
        }
        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon == null) return;
            currentWeapon = weapon;
            currentAmmo = weapon.MagazineSize;
            reserveAmmo = weapon.MaxAmmo;
            overheatLevel = 0f;
            isOverheated = false;
            OnWeaponSwitched?.Invoke();
            Debug.Log($"[WeaponSystem] Equipped: {weapon.WeaponName}");
        }
        public void Fire()
        {
            if (currentWeapon == null || isReloading || isOverheated) return;
            if (currentAmmo <= 0) { Reload(); return; }
            float fireRateInterval = currentWeapon.FireRate > 0f ? 60f / currentWeapon.FireRate : 0f;
            if (Time.time - lastFireTime < fireRateInterval) return;
            currentAmmo--;
            lastFireTime = Time.time;
            overheatLevel += currentWeapon.HeatPerShot;
            if (firePoint != null) PerformRaycast();
            PlayFireEffects();
            ApplyRecoil();
            OnWeaponFired?.Invoke();
            if (overheatLevel >= currentWeapon.MaxHeat)
            {
                isOverheated = true;
                Invoke(nameof(Cooldown), currentWeapon.CooldownTime);
            }
        }
        public void Reload()
        {
            if (currentWeapon == null || isReloading || currentAmmo >= currentWeapon.MagazineSize || reserveAmmo <= 0) return;
            isReloading = true;
            reloadTimer = currentWeapon.ReloadTime;
            Invoke(nameof(CompleteReload), currentWeapon.ReloadTime);
        }
        private void CompleteReload()
        {
            if (currentWeapon == null) { isReloading = false; return; }
            int ammoNeeded = currentWeapon.MagazineSize - currentAmmo;
            int ammoToLoad = Mathf.Min(ammoNeeded, reserveAmmo);
            currentAmmo += ammoToLoad;
            reserveAmmo -= ammoToLoad;
            isReloading = false;
            OnWeaponReloaded?.Invoke();
        }
        private void PerformRaycast()
        {
            Vector3 fireDirection = firePoint.forward;
            float spread = currentWeapon.Spread;
            if (spread > 0) fireDirection += new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
            if (Physics.Raycast(firePoint.position, fireDirection, out RaycastHit hit, currentWeapon.MaxRange))
            {
                var target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(CalculateDamage(hit.distance), currentWeapon.DamageType);
                }
            }
        }
        private int CalculateDamage(float distance)
        {
            float effectiveRange = currentWeapon.EffectiveRange;
            float damageMultiplier = 1f;
            if (distance > effectiveRange && currentWeapon.MaxRange > effectiveRange)
                damageMultiplier = Mathf.Lerp(1f, 0.5f, (distance - effectiveRange) / (currentWeapon.MaxRange - effectiveRange));
            if (GetComponent<TechSystem>()?.IsTechActive(TechType.WeaponOverdrive) == true) damageMultiplier *= 2f;
            return Mathf.RoundToInt(currentWeapon.Damage * damageMultiplier);
        }
        private void PlayFireEffects() { muzzleFlash?.Play(); shellEjection?.Play(); }
        private void ApplyRecoil() { }
        private void Cooldown() { isOverheated = false; overheatLevel = 0f; }
        private void Update()
        {
            if (currentWeapon != null && overheatLevel > 0 && !isOverheated)
            {
                overheatLevel -= Time.deltaTime * currentWeapon.CooldownRate;
                overheatLevel = Mathf.Max(0, overheatLevel);
            }
        }
    }
    [System.Serializable]
    public class WeaponData
    {
        public string WeaponName; public WeaponType Type; public int Damage; public float FireRate; public int MagazineSize; public int MaxAmmo; public float EffectiveRange; public float MaxRange; public float Spread; public float ReloadTime; public float HeatPerShot; public float MaxHeat; public float CooldownRate; public float CooldownTime; public DamageType DamageType; public Sprite WeaponIcon; public GameObject WeaponModel;
    }
    public enum WeaponType { PlasmaRifle, ThermalPistol, ParticleSMG, QuantumSniper, HackPistol, NanobotLauncher }
    public interface IDamageable { void TakeDamage(int damage, DamageType damageType); }
}
