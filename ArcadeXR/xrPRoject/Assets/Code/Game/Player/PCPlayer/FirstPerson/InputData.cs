using System.Numerics;
using UnityEngine;

/// <summary>
/// component to control a player with mouse and keyboard
/// </summary>
namespace Player.FirstPerson //use this to have include paths like in cpp!, includes all classes!
{
    class InputData
    {
        private bool MouseLeftDown = false;
        private bool MouseRightDown = false;



        public InputData()
        {

        }

        public void UpdateMouse(bool left, bool right)
        {
            MouseLeftDown = left;
            MouseRightDown = right;
        }

        public bool IsMouseLeftDown()
        {
            return MouseLeftDown;
        }

        public bool IsMouseRightDown()
        {
            return MouseRightDown;
        }


    };  



};