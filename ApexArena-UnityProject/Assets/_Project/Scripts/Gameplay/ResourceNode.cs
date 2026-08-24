using UnityEngine;
using ApexArena.Gameplay;

namespace ApexArena.Gameplay
{
    /// <summary>
    /// عقدة موارد - يمكن جمعها من اللاعبين
    /// Resource Node - can be collected by players
    /// </summary>
    public class ResourceNode : MonoBehaviour
    {
        [Header("Resource")]
        [SerializeField] private ResourceType resourceType = ResourceType.Energy;
        [SerializeField] private int amount = 50;
        [SerializeField] private bool respawns = false;
        [SerializeField] private float respawnTime = 60f;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private ParticleSystem collectEffect;
        [SerializeField] private Light glowLight;

        private bool isCollected = false;
        private float respawnTimer;

        private void Start()
        {
            SetupVisuals();
        }

        private void Update()
        {
            if (isCollected && respawns)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0)
                {
                    Respawn();
                }
            }
        }

        public void Collect(ResourceManager collector)
        {
            if (isCollected || collector == null) return;

            collector.AddResource(resourceType, amount);

            isCollected = true;
            respawnTimer = respawnTime;

            // تأثيرات
            collectEffect?.Play();

            // إخفاء
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (glowLight != null) glowLight.enabled = false;

            GetComponent<Collider>()?.enabled = false;

            Debug.Log($"[ResourceNode] Collected {amount} {resourceType}");
        }

        private void Respawn()
        {
            isCollected = false;

            if (meshRenderer != null) meshRenderer.enabled = true;
            if (glowLight != null) glowLight.enabled = true;

            GetComponent<Collider>()?.enabled = true;
        }

        private void SetupVisuals()
        {
            Color color = resourceType switch
            {
                ResourceType.Energy => new Color(0f, 0.9f, 1f),
                ResourceType.Data => new Color(0.73f, 0.4f, 1f),
                ResourceType.Materials => new Color(1f, 0.42f, 0.21f),
                ResourceType.Reputation => new Color(1f, 0.84f, 0f),
                _ => Color.white
            };

            if (glowLight != null)
            {
                glowLight.color = color;
            }

            if (meshRenderer != null)
            {
                meshRenderer.material.color = color;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = resourceType switch
            {
                ResourceType.Energy => Color.cyan,
                ResourceType.Data => Color.magenta,
                ResourceType.Materials => Color.red,
                ResourceType.Reputation => Color.yellow,
                _ => Color.white
            };

            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
