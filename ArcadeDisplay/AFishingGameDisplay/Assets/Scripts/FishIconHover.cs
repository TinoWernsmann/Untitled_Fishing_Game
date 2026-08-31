using UnityEngine;

public class UIHoverIconManager : MonoBehaviour
{
    public static UIHoverIconManager Instance;

    public RectTransform fishIcon;
    
    private void Awake()
    {
        Instance = this;
        fishIcon.gameObject.SetActive(false);
    }

    public void ShowIcon(RectTransform target)
    {
        fishIcon.gameObject.SetActive(true);

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 left = corners[0];

        // Mittelpunkt Y berechnen
        float centerY = (corners[0].y + corners[1].y) * 0.5f;

        fishIcon.position = new Vector3(
            left.x - 5f,   // links + padding
            centerY,
            fishIcon.position.z
        );
    }

    public void HideIcon()
    {
        fishIcon.gameObject.SetActive(false);
    }
}
