using UnityEngine;

namespace ApexArena.ScriptableObjects
{
    /// <summary>
    /// بيانات المنطقة القابلة للإنشاء
    /// </summary>
    [CreateAssetMenu(fileName = "NewZone", menuName = "Apex Arena/Zone")]
    public class ZoneDataSO : ScriptableObject
    {
        public ZoneType ZoneType = ZoneType.Industrial;
        public string ZoneName = "Industrial Zone";
        [TextArea] public string Description = "Abandoned factories with advanced tech";
        public int BaseDifficulty = 5;
        public int ResourceRichness = 9;
        public int TechDensity = 7;
        public int MaxPlayers = 30;
        public Color ZoneColor = Color.cyan;
        public AudioClip AmbientSound;
        public GameObject ZonePrefab;
    }

    public enum ZoneType
    {
        Industrial,
        Military,
        Forest,
        Danger
    }
}
