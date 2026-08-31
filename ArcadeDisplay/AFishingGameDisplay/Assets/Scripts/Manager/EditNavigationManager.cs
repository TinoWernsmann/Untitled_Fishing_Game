using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; 
using System.Collections; 

public class IPManager : MonoBehaviour
{
    [Header("Text-Blöcke")]
    public TMP_Text[] blocks;

    [Header("Werte-Grenzen")]
    public int maxValue = 255;
    public int minValue = 0;

    private int currentIndex = 0;
    private bool isEditing = false;
    
    private Coroutine scrollCoroutine;

    public void HandleClick() 
    {
        if (isEditing || blocks.Length == 0) return;

        isEditing = true;
        EventSystem.current.sendNavigationEvents = false;
        
        if (Manager.Input.InputManager.Instance != null)
        {
            Manager.Input.InputManager.Instance.ToggleSettingsInput(true);

            Manager.Input.InputManager.Instance.OnSettingsUpStart += StartScrollUp;
            Manager.Input.InputManager.Instance.OnSettingsUpCanceled += StopScroll;
            
            Manager.Input.InputManager.Instance.OnSettingsDownStart += StartScrollDown;
            Manager.Input.InputManager.Instance.OnSettingsDownCanceled += StopScroll;


            Manager.Input.InputManager.Instance.OnSettingLeft += OnInputLeft;
            Manager.Input.InputManager.Instance.OnSettingRight += OnInputRight;
            
            Manager.Input.InputManager.Instance.OnSettingConfirm += ExitEditMode;
        }
        
        currentIndex = 0; 
        UpdateHighlight(); 
    }

    private void ExitEditMode()
    {
        if (!isEditing) return;
        isEditing = false;
        
        StopScroll(); // Sicherheitshalber Scrollen stoppen!
        EventSystem.current.sendNavigationEvents = true;
        
        if (Manager.Input.InputManager.Instance != null)
        {
            Manager.Input.InputManager.Instance.ToggleSettingsInput(false);

            Manager.Input.InputManager.Instance.OnSettingsUpStart -= StartScrollUp;
            Manager.Input.InputManager.Instance.OnSettingsUpCanceled -= StopScroll;
            
            Manager.Input.InputManager.Instance.OnSettingsDownStart -= StartScrollDown;
            Manager.Input.InputManager.Instance.OnSettingsDownCanceled -= StopScroll;

            Manager.Input.InputManager.Instance.OnSettingLeft -= OnInputLeft;
            Manager.Input.InputManager.Instance.OnSettingRight -= OnInputRight;
            
            Manager.Input.InputManager.Instance.OnSettingConfirm -= ExitEditMode;
        }
        
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        RemoveHighlight(); 
    }

    private void StartScrollUp() => StartScroll(1);
    private void StartScrollDown() => StartScroll(-1);

    private void StartScroll(int amount)
    {
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
        scrollCoroutine = StartCoroutine(ScrollRoutine(amount));
    }

    private void StopScroll()
    {
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
    }

    private IEnumerator ScrollRoutine(int amount)
    {
       
        ChangeValue(amount);

        yield return new WaitForSeconds(0.4f);

        int ticks = 0;
        
        while (true)
        {
            ChangeValue(amount);
            ticks++;

            float waitTime = (ticks > 15) ? 0.02f : 0.08f;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnInputLeft() => ChangeBlock(-1);
    private void OnInputRight() => ChangeBlock(1);

    private void ChangeBlock(int direction)
    {
        if (blocks.Length <= 1) return;
        blocks[currentIndex].color = Color.orange;
        currentIndex += direction;
        if (currentIndex >= blocks.Length) currentIndex = 0;
        if (currentIndex < 0) currentIndex = blocks.Length - 1;
        UpdateHighlight();
    }

    private void ChangeValue(int amount)
    {
        int currentValue = 0;
        int.TryParse(blocks[currentIndex].text, out currentValue);
        currentValue += amount;
        if (currentValue > maxValue) currentValue = minValue;
        if (currentValue < minValue) currentValue = maxValue;
        blocks[currentIndex].text = currentValue.ToString();
    }

    private void UpdateHighlight() => blocks[currentIndex].color = Color.white;

    private void RemoveHighlight()
    {
        foreach (var b in blocks) b.color = Color.orange;
    }
}