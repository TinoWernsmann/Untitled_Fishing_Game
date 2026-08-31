using UnityEngine;

public class SellButtonManager : MonoBehaviour
{
    public InventoryUI inventoryUI;
    //Show Fish Inventory 
    public bool sellMode = false;
   
    public void HandleClick()
    {
        if (sellMode)
        {
            if (inventoryUI != null)
            {
                inventoryUI.SellAllFishes();
            }
            else Debug.Log("no inventoryui found");
        }
        sellMode = true;
    }
    public void LeaveSellMode()
    {
        sellMode = false;
    }

}
