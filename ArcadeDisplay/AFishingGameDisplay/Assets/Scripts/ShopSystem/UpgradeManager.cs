using System.Collections.Generic;
using UnityEngine;

/* 
<summary> 
Verwaltet die Upgrade-Logik und die aktuellen Upgrade-Werte der Angel zur Kommunikation mit FishingRodParameter
Beim Upgrade der Angel werden mehrere Strings nacheinander gesendet, um die betroffenen Params zu setzen
Beim Upgrade des Baits ist nur die depths relevant. 
Problem: Casten der Float Werte auf int ist bei Übergabe nötig - dadurch kann z.B. MiniGameTime derzeit nicht ohne weitere Umrechnung um Milisekunden verändert werden. 
<summary> 
 */
public class UpgradeManager : MonoBehaviour
{
    [Header("Current Upgrade Levels")]
    public int RodUpgradeLevel = 0;
    public int BaitUpgradeLevel = 0;

    [Header("Maximum Upgrade Levels")]
    public int MaxRodUpgradeLevel = 3;
    public int MaxBaitUpgradeLevel = 3;

    [Header("Rod Parameters")]
    public float MiniGameRadius = 4.0f;
    public float MiniGameTime = 1.0f;
    public float PullStrength = 1.0f;

    [Header("Bait Parameters")]
    public int DepthLevel = 1;

    public List<string> UpgradeRod()
    {

        if (RodUpgradeLevel >= MaxRodUpgradeLevel)
            return null;

        RodUpgradeLevel++;

        // ===== Upgrade Werte für Balancing =====
        MiniGameRadius += 1f;
        MiniGameTime += 1f;
        PullStrength += 1f;

        return new List<string>()
        {
            $"rodparams_minigameradius_{(int)MiniGameRadius}",
            $"rodparams_minigametime_{(int)MiniGameTime}",
            $"rodparams_pullstrength_{(int)PullStrength}"
        };
    }

    public List<string> UpgradeBait()
    {
        if (BaitUpgradeLevel >= MaxBaitUpgradeLevel)
            return null;

        BaitUpgradeLevel++;

        DepthLevel++;

        return new List<string>()
        {
            $"rodparams_depthlevel_{DepthLevel}"
        };
    }
}
