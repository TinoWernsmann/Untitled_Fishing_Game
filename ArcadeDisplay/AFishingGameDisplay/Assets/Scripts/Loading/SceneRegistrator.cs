using Manager.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace Loading.SceneRegi
{
    public class SceneRegistrator : MonoBehaviour
    {
        [SerializeField] private Button[] _buttons;
        [SerializeField] private bool _doNotSelectFirstElementAtStart;

        private void Start()
        {
            NavigationManager.Instance.UpdateAvailableNavElements(_buttons, _doNotSelectFirstElementAtStart);
        }
    }
}
