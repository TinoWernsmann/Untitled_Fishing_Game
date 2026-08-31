using System;
using Game.FishingRod.transfer;
using Horror.Manager;
using Manager.Timer;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    //private int _currentDepth = 1;

    //public static event Action<int> OnDepthUpgraded;

    public void UpgradeDepth()
    {
        Debug.Log("LayerUpgrade");
        int currentDepth = FishingRodParameters.Instance.depthLevel;
        if (currentDepth >= 3)
        {
            Debug.LogWarning("=== MAX DEPTH REACHED (3/3) - NO MORE UPGRADES ===");
            return;
        }

        FishingRodParameters.Instance.IncrementDepth();
        int newDepth = FishingRodParameters.Instance.depthLevel;
        float newMaxHorror = 33f * newDepth;
        HorrorManager.Instance.SetMaxHorrorValue(newMaxHorror);

        Debug.Log($"=== UPGRADE {newDepth-1}/2: Depth {newDepth}/3, Max Horror unlocked: {newMaxHorror}% ===");
    }
}
