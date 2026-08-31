using UnityEngine;

public enum HorrorType
{
    Texture,
    Audio,
    Both,
    Activation
}

[CreateAssetMenu(menuName = "Horror/ElementChange")]
public class HorrorMapChange : ScriptableObject
{
    public HorrorType type;
    public string changeName;
    public float changeThreshold;
    public Texture2D textureToChange;
    public AudioClip musicToPlay;
    [Tooltip("IDs of the MapChangers(s) to target with this change. Leave empty if every MapChanger should react.")]
    public string[] targetIDs;
}
