using Horror.Changer;
using Horror.Manager;
using UnityEngine;

public class MapElementActivator : MapChanger
{
    [SerializeField] private GameObject _objectToActivate;

    private void Start()
    {
        HorrorManager.Instance.OnHorrorChange += HandleHorrorChange;
    }

    protected override void HandleHorrorChange(HorrorMapChange newChange)
    {
        base.HandleHorrorChange(newChange);
        if (newChange.type == HorrorType.Activation)
        {
            HandleActivation();
        }
    }

    private void HandleActivation()
    {
        _objectToActivate.SetActive(!_objectToActivate.activeSelf);
    }
}
