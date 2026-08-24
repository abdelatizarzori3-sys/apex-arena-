using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// البيئة الديناميكية - تدير التغييرات البيئية والمناطق
    /// Dynamic Environment - manages environmental changes and zones
    /// </summary>
    public class DynamicEnvironment : MonoBehaviour
    {
        public static DynamicEnvironment Instance { get; private set; }

        [Header("Zones")]
        [SerializeField] private List<Zone> zones = new List<Zone>();
        [SerializeField] private float mutationCooldown = 180f;

        [Header("Danger Zone")]
        [SerializeField] private float dangerExpandRate = 0.15f;
        [SerializeField] private float maxDangerRadius = 500f;
        [SerializeField] private float radiationDamagePerSecond = 5f;

        [Header("Terrain")]
        [SerializeField] private Terrain terrain;
        [SerializeField] private float terrainChangeSpeed = 2f;

        private float lastMutationTime;
        private Dictionary<ZoneType, Zone> zoneMap = new Dictionary<ZoneType, Zone>();

        public event System.Action<Zone, ZoneMutation> OnZoneMutated;
        public event System.Action OnTerrainChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeZones();
            lastMutationTime = Time.time;
        }

        private void Update()
        {
            ApplyRadiationDamage();
        }

        private void InitializeZones()
        {
            zoneMap.Clear();
            foreach (var zone in zones)
            {
                zoneMap[zone.ZoneType] = zone;
                zone.Initialize();
            }
        }

        /// <summary>
        /// تفعيل تحول بيئي - يُستدعى من GameManager
        /// </summary>
        public void MutateZones()
        {
            if (Time.time - lastMutationTime < mutationCooldown) return;

            // اختيار منطقة عشوائية للتحول
            Zone targetZone = zones[Random.Range(0, zones.Count)];
            ZoneMutation mutation = GenerateMutation(targetZone);

            ApplyMutation(targetZone, mutation);

            // توسع المنطقة الخطرة
            ExpandDangerZone();

            lastMutationTime = Time.time;
            OnZoneMutated?.Invoke(targetZone, mutation);

            Debug.Log($"[DynamicEnvironment] {targetZone.ZoneName} mutated: {mutation.MutationType}");
        }

        private ZoneMutation GenerateMutation(Zone zone)
        {
            var mutation = new ZoneMutation
            {
                MutationType = (MutationType)Random.Range(0, 6),
                Duration = Random.Range(60f, 300f),
                Intensity = Random.Range(0.5f, 2f)
            };

            switch (mutation.MutationType)
            {
                case MutationType.TerrainShift:
                    mutation.Description = "New paths opened, terrain elevation changed";
                    break;
                case MutationType.WeatherChange:
                    mutation.Description = "Weather pattern shifted, visibility affected";
                    break;
                case MutationType.HazardSpawn:
                    mutation.Description = "New hazards appeared in the zone";
                    break;
                case MutationType.ResourceBurst:
                    mutation.Description = "Resource nodes spawned";
                    break;
                case MutationType.TechSurge:
                    mutation.Description = "Technology nodes activated";
                    break;
                case MutationType.RadiationSpike:
                    mutation.Description = "Radiation levels increased";
                    break;
            }

            return mutation;
        }

        private void ApplyMutation(Zone zone, ZoneMutation mutation)
        {
            zone.ApplyMutation(mutation);

            // تعديل التضاريس
            if (mutation.MutationType == MutationType.TerrainShift)
            {
                StartCoroutine(AnimateTerrainChange(zone));
            }
        }

        private void ExpandDangerZone()
        {
            if (zoneMap.TryGetValue(ZoneType.Danger, out var dangerZone))
            {
                dangerZone.Expand(dangerExpandRate);
            }
        }

        private void ApplyRadiationDamage()
        {
            if (!zoneMap.TryGetValue(ZoneType.Danger, out var dangerZone)) return;

            Collider[] playersInDanger = Physics.OverlapSphere(
                dangerZone.transform.position, 
                dangerZone.CurrentRadius, 
                LayerMask.GetMask("Player")
            );

            foreach (var col in playersInDanger)
            {
                var player = col.GetComponent<PlayerController>();
                if (player != null && player.IsAlive)
                {
                    player.TakeDamage(Mathf.RoundToInt(radiationDamagePerSecond * Time.deltaTime), DamageType.Radiation);
                }
            }
        }

        private IEnumerator AnimateTerrainChange(Zone zone)
        {
            if (terrain == null) yield break;

            float[,] originalHeights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
            float[,] targetHeights = GenerateNewHeights(originalHeights, zone);

            float elapsed = 0f;
            while (elapsed < terrainChangeSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / terrainChangeSpeed;

                float[,] currentHeights = BlendHeights(originalHeights, targetHeights, t);
                terrain.terrainData.SetHeights(0, 0, currentHeights);

                yield return null;
            }

            OnTerrainChanged?.Invoke();
        }

        private float[,] GenerateNewHeights(float[,] original, Zone zone)
        {
            float[,] newHeights = (float[,])original.Clone();
            // خوارزمية بسيطة لتعديل الارتفاعات
            int centerX = Random.Range(0, newHeights.GetLength(0));
            int centerY = Random.Range(0, newHeights.GetLength(1));
            float radius = Random.Range(10f, 50f);
            float heightChange = Random.Range(-0.1f, 0.1f);

            for (int x = 0; x < newHeights.GetLength(0); x++)
            {
                for (int y = 0; y < newHeights.GetLength(1); y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist < radius)
                    {
                        float factor = 1f - (dist / radius);
                        newHeights[x, y] += heightChange * factor;
                    }
                }
            }

            return newHeights;
        }

        private float[,] BlendHeights(float[,] a, float[,] b, float t)
        {
            float[,] result = new float[a.GetLength(0), a.GetLength(1)];
            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int y = 0; y < a.GetLength(1); y++)
                {
                    result[x, y] = Mathf.Lerp(a[x, y], b[x, y], t);
                }
            }
            return result;
        }

        public Zone GetZone(ZoneType type)
        {
            return zoneMap.TryGetValue(type, out var zone) ? zone : null;
        }
    }

    public enum ZoneType
    {
        Industrial,
        Military,
        Forest,
        Danger
    }

    public enum MutationType
    {
        TerrainShift,
        WeatherChange,
        HazardSpawn,
        ResourceBurst,
        TechSurge,
        RadiationSpike
    }

    [System.Serializable]
    public class ZoneMutation
    {
        public MutationType MutationType;
        public float Duration;
        public float Intensity;
        public string Description;
    }
}
