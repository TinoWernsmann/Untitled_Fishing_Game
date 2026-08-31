using UnityEngine;

using UnityEngine.InputSystem;
using Core.Game;
using System.Linq;
using System.Collections.Generic;
using System;

///base object for transform functions
///
namespace Core.Game
{
    public class GameObjectBase : MonoBehaviour
    {

        public void AttachComponentLocalSpace(Transform attachTo, GameObject objectToAttach)
        {
            if (!objectToAttach || !attachTo)
            {
                return;
            }
            objectToAttach.transform.SetParent(attachTo, false);
            //reset rotations
            objectToAttach.transform.localPosition = Vector3.zero;
            objectToAttach.transform.localRotation = Quaternion.identity;
            objectToAttach.transform.localScale = Vector3.one;
        }

        public virtual Collider[] GetAllColliders()
        {
            List<Collider> result = new List<Collider>();
            result.AddRange(GetComponentsInChildren<Collider>());
            result.Add(GetComponent<Collider>());


            return result.ToArray();
        }

        public void DetachFromParent()
        {
            transform.SetParent(null, true);

        }

        public virtual void SetLocation(Vector3 pos)
        {
            if (IsValid(pos))
            {
                transform.position = pos;
            }
        }

        public void SetScale(float x, float y, float z)
        {
            Vector3 other = new Vector3(x, y, z);
            SetScale(other);
        }
        
        public void SetScale(Vector3 other)
        {
            if (IsValid(other))
            {
                transform.localScale = other;
            }
            
        }
        
        protected bool IsValid(Vector3 v)
        {
            return
            float.IsFinite(v.x) &&
            float.IsFinite(v.y) &&
            float.IsFinite(v.z);
        }

        public void SetRotation(Quaternion quat)
        {
            transform.rotation = quat;
        }

        public void SetColliderEnabled(bool flag)
        {
            Collider[] array = GetAllColliders();
            foreach (Collider c in array)
            {
                if (c != null)
                {
                    c.enabled = flag;
                }
            }
        }




        //GetComponent<Collider>()


        public void GetAllChildrenRecursive(List<GameObject> outObjects)
        {
            GetAllChildrenRecursive(this.gameObject, outObjects);
        }

        public void GetAllChildrenRecursive(GameObject parent, List<GameObject> outObjects)
        {
            if (parent != null)
            {
                // 'true' schließt inaktive Kinder mit ein
                Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child != parent.transform)
                    {
                        GameObject childObject = child.gameObject;
                        if (childObject != null)
                        {
                            outObjects.Add(childObject);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Sucht rekursiv nach allen Komponenten vom Typ T in den Kindern (inkl. inaktiver Objekte).
        /// </summary>
        public void TGetComponentsInChildrenRecursive<T>(List<T> outComponents) where T : Component
        {
            TGetComponentsInChildrenRecursive(this.gameObject, outComponents);
        }

        public static void TGetComponentsInChildrenRecursive<T>(GameObject parent, List<T> outComponents) where T : Component
        {
            if (parent != null && outComponents != null)
            {
                // Unitys native Methode holt direkt alle Komponenten vom Typ T im gesamten Teilbaum
                T[] components = parent.GetComponentsInChildren<T>(true);
                foreach (T comp in components)
                {
                    // Verhindert, dass die Komponente des Parents selbst mit aufgenommen wird
                    if (comp.gameObject != parent)
                    {
                        outComponents.Add(comp);
                    }
                }
            }
        }
        
        public static T TFindFirstComponentInChildrenRecursive<T>(GameObject parent) where T : Component
        {
            List<T> outComponents = new List<T>();
            TGetComponentsInChildrenRecursive(parent, outComponents);
            if (outComponents.Count > 0)
            {
                return outComponents[0];
            }
            return null;
        }

            



    };
}