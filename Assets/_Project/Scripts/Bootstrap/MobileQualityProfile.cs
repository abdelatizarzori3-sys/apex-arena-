using UnityEngine;

namespace ApexArena.Bootstrap
{
    /// <summary>
    /// Applies conservative mobile defaults before the first scene starts.
    /// Final visual quality remains scalable per device instead of forcing
    /// desktop settings onto Android hardware.
    /// </summary>
    public static class MobileQualityProfile
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            if (Application.platform != RuntimePlatform.Android &&
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            var memoryGb = SystemInfo.systemMemorySize / 1024f;
            var highTier = memoryGb >= 6f && SystemInfo.graphicsMemorySize >= 2048;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = highTier ? 60 : 30;
            QualitySettings.lodBias = highTier ? 1.2f : 0.7f;
            QualitySettings.shadowDistance = highTier ? 35f : 15f;
            QualitySettings.shadowResolution = highTier
                ? ShadowResolution.Medium
                : ShadowResolution.Low;
            QualitySettings.antiAliasing = highTier ? 2 : 0;
            QualitySettings.realtimeReflectionProbes = highTier;
            QualitySettings.softParticles = highTier;
            QualitySettings.particleRaycastBudget = highTier ? 256 : 64;
        }
    }
}
