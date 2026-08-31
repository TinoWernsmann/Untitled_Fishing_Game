using System;
using Gameworld.Pond;
using UnityEngine;
using Game.FishingRod.transfer;
using UnityEngine.InputSystem;

namespace Debugging.Keyboard
{
    public class DebugKeyboard : MonoBehaviour
    {
        private InputSystem_Actions _input;
        private Pond _debugPond;

        public event Action<CatchData> OnFished;

        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.DebugKeyboard.Enable();
            _input.DebugKeyboard.GetFish.performed += OnGetFish;
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
            {
                BuyUpgradeDebug();
            }
        }

        private void OnGetFish(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //HorrorManager.Instance.AddValue(1.0f);

            CatchData caught = _debugPond.CaughtCatchable();

            if (caught != null)
            {
                OnFished?.Invoke(caught);
            }
            else
            {
                Debug.Log("Nichts gefangen!");
            }
        }

        public void ConnectPond(Pond pond)
        {
            _debugPond = pond;
        }

        private void BuyUpgradeDebug()
        {
            var upgradeManager = FindObjectOfType<UpgradeManager>();
            if (upgradeManager != null)
            {
                upgradeManager.UpgradeDepth();
                Debug.Log(">>> B KEY PRESSED: UPGRADE PURCHASED <<<");
            }
            else
            {
                Debug.LogError("!!! UpgradeManager NOT FOUND in scene !!!");
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.DebugKeyboard.GetFish.performed -= OnGetFish;
                _input.DebugKeyboard.Disable();
            }
        }
    }
}

