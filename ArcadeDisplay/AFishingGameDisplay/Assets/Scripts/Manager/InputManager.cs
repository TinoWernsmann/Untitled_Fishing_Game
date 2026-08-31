using System;
using UnityEngine;

namespace Manager.Input
{
    public class InputManager : MonoBehaviour
    {
        //for Settings View
        public bool IsEditing = false;
        public static InputManager Instance { get; private set; }

        private InputSystem_Actions _input;

        public event Action OnUp;
        public event Action OnDown;
        public event Action OnConfirm;
        public event Action OnNavUp;
        public event Action OnNavDown;
        public event Action OnNavConfirm;
        public event Action OnSettingConfirm;
        public event Action OnSettingUp;
        public event Action OnSettingDown;
        public event Action OnSettingLeft;
        public event Action OnSettingRight;
        public event Action OnSettingsUpStart;
        public event Action OnSettingsUpCanceled;
        public event Action OnSettingsDownStart;
        public event Action OnSettingsDownCanceled;

        public static event Action<InputManager> OnReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _input = new InputSystem_Actions();
            Instance = this;
            DontDestroyOnLoad(gameObject);

            OnReady?.Invoke(this);
        }

        private void OnEnable()
        {
            if (_input == null) return;

            _input.Name.Up.performed += UpPerformed;
            _input.Name.Down.performed += DownPerformed;
            _input.Name.Confirm.performed += ConfirmPerformed;
            _input.Nav.Up.performed += NavUp;
            _input.Nav.Down.performed += NavDown;
            _input.Nav.Confirm.performed += NavConfirm;
            _input.Settings.Confirm.performed += SettingConfirm;
            _input.Settings.Up.performed += SettingUp;
            _input.Settings.Down.performed += SettingDown;
            _input.Settings.Left.performed += SettingLeft;
            _input.Settings.Right.performed += SettingRight;
            // Wenn die Taste gedrückt wird:
            _input.Settings.Up.started += ctx => OnSettingsUpStart?.Invoke();
            _input.Settings.Down.started += ctx => OnSettingsDownStart?.Invoke();

            // Wenn die Taste wieder LOSGELASSEN wird:
            _input.Settings.Up.canceled += ctx => OnSettingsUpCanceled?.Invoke();
            _input.Settings.Down.canceled += ctx => OnSettingsDownCanceled?.Invoke();
        }

        private void SettingConfirm(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnSettingConfirm?.Invoke();
        }
        private void SettingUp(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnSettingUp?.Invoke();
        }
        private void SettingDown(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnSettingDown?.Invoke();
        }
        private void SettingLeft(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnSettingLeft?.Invoke();
        }
        private void SettingRight(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnSettingRight?.Invoke();
        }
        private void NavConfirm(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnNavConfirm?.Invoke();
        }

        private void NavDown(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnNavDown?.Invoke();
        }

        private void NavUp(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            OnNavUp?.Invoke();
        }

        private void ConfirmPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            OnConfirm?.Invoke();
        }

        private void DownPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

            OnDown?.Invoke();
        }

        private void UpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            OnUp?.Invoke();
        }

        public void ToggleNameInput(bool isOn)
        {
            if (isOn)
            {
                _input.Nav.Disable();
                _input.Name.Enable();
                _input.Settings.Disable();
            }
            else
            {
                _input.Nav.Enable();
                _input.Name.Disable();
                _input.Settings.Disable();
            }
        }
        public void ToggleSettingsInput(bool IsEditing)
        {
            if (IsEditing)
            {
                _input.Nav.Disable();
                _input.Name.Disable();
                _input.Settings.Enable();
            }
            else
            {
                _input.Nav.Enable();
                _input.Name.Disable();
                _input.Settings.Enable();
            }
        }

        public void EnableNavInput()
        {
            _input.Nav.Enable();
        }

        private void OnDisable()
        {
            if (_input == null) return;

            _input.Name.Up.performed -= UpPerformed;
            _input.Name.Down.performed -= DownPerformed;
            _input.Name.Confirm.performed -= ConfirmPerformed;
            _input.Nav.Up.performed -= NavUp;
            _input.Nav.Down.performed -= NavDown;
            _input.Nav.Confirm.performed -= NavConfirm;
            _input.Name.Disable();
            _input.Nav.Disable();
        }
    }
}

