using TMPro;
using UnityEngine;

public class CurrentUpgradesUI : MonoBehaviour
{
    [SerializeField]
    private UpgradeManager upgradeManager;

    [SerializeField]
    private TMP_Text upgradeStatsText;

    private void Start()
    {
        UpdateUpgradeStats();
    }

    public void UpdateUpgradeStats()
    {
        upgradeStatsText.text =
            $"Rod Upgrade: {upgradeManager.RodUpgradeLevel} / {upgradeManager.MaxRodUpgradeLevel}\n" +
            $"Bait Upgrade: {upgradeManager.BaitUpgradeLevel} / {upgradeManager.MaxBaitUpgradeLevel}";
    }
}