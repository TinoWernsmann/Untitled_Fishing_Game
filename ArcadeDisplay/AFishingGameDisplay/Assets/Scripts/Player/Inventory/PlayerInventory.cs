using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public event Action OnInventoryChanged;

    private readonly List<InventoryItem> fishItems = new();

    public IReadOnlyList<InventoryItem> StoredFish => fishItems;

    public void AddFish(FishData fish, int amount = 1)
    {
        if (fish == null || amount <= 0)
            return;

        InventoryItem existingItem = fishItems.Find(item => item.Fish == fish);

        if (existingItem != null)
        {
            existingItem.Amount += amount;
        }
        else
        {
            fishItems.Add(new InventoryItem(fish, amount));
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveOneFish(InventoryItem item)
    {
        if (item == null)
            return;

        item.Amount--;

        if (item.Amount <= 0)
        {
            fishItems.Remove(item);
        }

        OnInventoryChanged?.Invoke();
    }

    // evtl. für später nutzlich, um z.B. die Gesamtmenge an Fischen im Inventar zu ermitteln.
    public int GetTotalFishAmount()
    {
        int total = 0;

        foreach (InventoryItem item in fishItems)
        {
            total += item.Amount;
        }

        return total;
    }
    
    public void ClearInventory()
    {
        fishItems.Clear();

        OnInventoryChanged?.Invoke();
    }
}