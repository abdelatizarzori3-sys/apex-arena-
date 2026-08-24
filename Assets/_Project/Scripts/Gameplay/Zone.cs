using UnityEngine;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// منطقة في الحلبة - تحتوي على خصائص فريدة
    /// Zone in the arena - contains unique properties
    /// </summary>
    public class Zone : MonoBehaviour
    {
        [Header("Zone Identity")]
        public ZoneType ZoneType;
        public string ZoneName;
        [TextArea] public string ZoneDescription;

        [Header("Properties")]
        [SerializeField] private float baseRadius = 100f;
        [SerializeField] private float currentRadius;
        [SerializeField] private int baseDifficulty = 5;
        [SerializeField] private int resourceRichness = 5;
        [SerializeField] private int techDensity = 5;
        [SerializeField] private int maxPlayers = 25;

        [Header("Visuals")]
        [SerializeField] private Color zoneColor = Color.cyan;
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private Light zoneLight;

        [Header("Mutation")]
        [SerializeField] private bool isMutating;
        [SerializeField] private ZoneMutation activeMutation;

        public float CurrentRadius => currentRadius;
        public bool IsMutating => isMutating;

        public void Initialize()
        {
            currentRadius = baseRadius;
            SetupVisuals();
        }

        private void SetupVisuals()
        {
            switch (ZoneType)
            {
                case ZoneType.Industrial:
                    zoneColor = new Color(0f, 0.9f, 1f, 0.3f); // Cyan
                    break;
                case ZoneType.Military:
                    zoneColor = new Color(1f, 0.42f, 0.21f, 0.3f); // Orange
                    break;
                case ZoneType.Forest:
                    zoneColor = new Color(0f, 1f, 0.53f, 0.3f); // Green
                    break;
                case ZoneType.Danger:
                    zoneColor = new Color(1f, 0.16f, 0.43f, 0.3f); // Red
                    break;
            }

            if (zoneLight != null)
            {
                zoneLight.color = zoneColor;
            }
        }

        public void ApplyMutation(ZoneMutation mutation)
        {
            activeMutation = mutation;
            isMutating = true;

            // تطبيق التأثيرات
            switch (mutation.MutationType)
            {
                case MutationType.TerrainShift:
                    resourceRichness += 2;
                    break;
                case MutationType.ResourceBurst:
                    SpawnResourceNodes();
                    break;
                case MutationType.TechSurge:
                    techDensity += 3;
                    break;
                case MutationType.RadiationSpike:
                    baseDifficulty += 2;
                    break;
            }

            StartCoroutine(MutationCountdown(mutation.Duration));
        }

        private System.Collections.IEnumerator MutationCountdown(float duration)
        {
            yield return new WaitForSeconds(duration);
            isMutating = false;
            activeMutation = null;
        }

        public void Expand(float rate)
        {
            currentRadius = Mathf.Min(currentRadius * (1f + rate), baseRadius * 2f);
        }

        private void SpawnResourceNodes()
        {
            int nodeCount = Mathf.RoundToInt(resourceRichness * 2f);
            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 randomPos = transform.position + Random.insideUnitSphere * currentRadius;
                randomPos.y = terrain ? terrain.SampleHeight(randomPos) : 0f;

                // TODO: Instantiate resource prefab
                Debug.Log($"[Zone] Resource node spawned at {randomPos}");
            }
        }

        private Terrain terrain;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }
}
