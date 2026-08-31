using Manager.Input;
using Saving.Save;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Manager.Navigation
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }


        private InputManager _input;
        private List<Button> _navElements;
        private Button _currentNavElement;
        private int _currentNavIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var go = new GameObject("NavigationManager");
            Instance = go.AddComponent<NavigationManager>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            _navElements = new List<Button>();
        }

        private void OnEnable()
        {
            if (InputManager.Instance == null)
            {
                InputManager.OnReady += GameInputReady;
            }
            else
            {
                GameInputReady(InputManager.Instance);
            }
        }

        private void GameInputReady(InputManager input)
        {
            InputManager.OnReady -= GameInputReady;
            _input = input;
            _input.OnNavConfirm += HandleConfirm;
            _input.OnNavDown += HandleNavDown;
            _input.OnNavUp += HandleNavUp;
            _input.EnableNavInput();
        }

        private void HandleNavUp()
        {
            if (_currentNavIndex == 0) return;

            _currentNavIndex--;
            SelectNewElement();
        }

        private void HandleNavDown()
        {
            if (_currentNavIndex == _navElements.Count - 1) return;

            _currentNavIndex++;
            SelectNewElement();
        }

        private void SelectNewElement()
        {
            DemarkSelectedNavElement();
            _currentNavElement = _navElements[_currentNavIndex];
            MarkSelectedNavElement();
        }

        private void MarkSelectedNavElement()
        {
            if (_currentNavElement.TryGetComponent<ButtonHover>(out ButtonHover hover))
            {
                hover.SelectButton();
            }
        }

        private void DemarkSelectedNavElement()
        {
            if (_currentNavElement.TryGetComponent<ButtonHover>(out ButtonHover hover))
            {
                hover.DeselectButton();
            }
        }

        private void HandleConfirm()
        {
            _currentNavElement.onClick.Invoke();
        }

        public void UpdateAvailableNavElements(Button[] elements, bool isOff)
        {
            if (elements == null || elements.Length == 0)
            {
                Debug.Log("No valid Nav Objects given!");
                return;
            }

            _navElements = elements.ToList();

            _currentNavIndex = 0;

            if (isOff) return;
            SelectFirstNavElement();
        }

        public void SelectFirstNavElement()
        {
            _currentNavElement = _navElements[_currentNavIndex];
            MarkSelectedNavElement();
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.OnNavConfirm -= HandleConfirm;
            _input.OnNavDown -= HandleNavDown;
            _input.OnNavUp -= HandleNavUp;
        }
    }
}
