using UnityEngine;
using UnityEngine.InputSystem;

public class InputReceiver : MonoBehaviour
{


    public InputActionProperty inputActionLeftSelectValue; //wie weit ist der controller eingepressed (der abzug)
    public InputActionProperty inputActionLeftSelectButton; //ist gedrückt, ist nicht gedrückt


    //new
    public InputActionAsset asset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputAction action = inputActionLeftSelectValue.action;
        if (action != null)
        {
            float value = inputActionLeftSelectValue.action.ReadValue<float>();

            string printLine = "Hallo Linker Controller!" + value;
            //Debug.Log(printLine);

            DebugHelper.SetMessage(printLine);    
        }
        DebugHelper.SetMessage("NONE");    


    }


    
    
    








}
