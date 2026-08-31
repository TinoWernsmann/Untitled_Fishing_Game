using UnityEngine;

namespace Core.Catching
{
    public class Trash : CatchBase
    {
        protected override void OnObjectCaught()
        {
            base.OnObjectCaught();
            Debug.Log("Muell gefangen!");
        }
    } 
}
