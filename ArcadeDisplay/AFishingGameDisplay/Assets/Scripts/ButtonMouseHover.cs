using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform target;

    [Header("Optional Cat Animation")]
    public Animator catAnimator;
    public string animatorBool = "isScared";

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIHoverIconManager.Instance.ShowIcon(target);

        if (catAnimator != null)
        {
            catAnimator.SetBool(animatorBool, true);
        }
    }

    public void SelectButton()
    {
        UIHoverIconManager.Instance.ShowIcon(target);

        if (catAnimator != null)
        {
            catAnimator.SetBool(animatorBool, true);
        }
    }

    public void DeselectButton()
    {
        UIHoverIconManager.Instance.HideIcon();

        if (catAnimator != null)
        {
            catAnimator.SetBool(animatorBool, false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIHoverIconManager.Instance.HideIcon();

        if (catAnimator != null)
        {
            catAnimator.SetBool(animatorBool, false);
        }
    }
}