using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ApexArena.Gameplay;
using ApexArena.Core;

namespace ApexArena.UI
{
    /// <summary>
    /// مدير واجهة المستخدم - يدير HUD واللوحات
    /// UI Manager - manages HUD and panels
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI dataText;
        [SerializeField] private TextMeshProUGUI materialsText;
        [SerializeField] private TextMeshProUGUI reputationText;

        [Header("Minimap")]
        [SerializeField] private RectTransform minimap;
        [SerializeField] private RectTransform playerIcon;

        [Header("Tech Panel")]
        [SerializeField] private GameObject techPanel;
        [SerializeField] private Transform techGrid;

        [Header("Alliance Panel")]
        [SerializeField] private GameObject alliancePanel;
        [SerializeField] private Transform allianceList;

        [Header("Log")]
        [SerializeField] private Transform logContainer;
        [SerializeField] private GameObject logEntryPrefab;

        [Header("Player Reference")]
        [SerializeField] private PlayerController player;

        private void Start()
        {
            if (player == null)
                player = FindObjectOfType<PlayerController>();

            if (player != null)
            {
                player.OnHealthChanged += UpdateHealth;

                var resources = player.GetComponent<ResourceManager>();
                if (resources != null)
                {
                    resources.OnResourceChanged += UpdateResource;
                }
            }

            GameManager.Instance?.OnMatchTimeUpdated += UpdateMatchTime;
        }

        private void UpdateHealth(int current, int max)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
            healthText.text = $"{current}/{max}";
        }

        private void UpdateResource(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Energy:
                    energyText.text = $"⚡ {amount}";
                    break;
                case ResourceType.Data:
                    dataText.text = $"📊 {amount}";
                    break;
                case ResourceType.Materials:
                    materialsText.text = $"🔧 {amount}";
                    break;
                case ResourceType.Reputation:
                    reputationText.text = $"⭐ {amount}%";
                    break;
            }
        }

        private void UpdateMatchTime(float elapsed)
        {
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            // TODO: Update timer text
        }

        public void AddLogEntry(string message, LogType type = LogType.Info)
        {
            if (logEntryPrefab == null || logContainer == null) return;

            var entry = Instantiate(logEntryPrefab, logContainer);
            var text = entry.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";

                switch (type)
                {
                    case LogType.Warning:
                        text.color = Color.yellow;
                        break;
                    case LogType.Danger:
                        text.color = Color.red;
                        break;
                    case LogType.Success:
                        text.color = Color.green;
                        break;
                    default:
                        text.color = Color.white;
                        break;
                }
            }

            // إزالة القديم
            if (logContainer.childCount > 20)
            {
                Destroy(logContainer.GetChild(0).gameObject);
            }
        }

        public void ToggleTechPanel()
        {
            techPanel?.SetActive(!techPanel.activeSelf);
        }

        public void ToggleAlliancePanel()
        {
            alliancePanel?.SetActive(!alliancePanel.activeSelf);
        }
    }

    public enum LogType
    {
        Info,
        Warning,
        Danger,
        Success
    }
}
