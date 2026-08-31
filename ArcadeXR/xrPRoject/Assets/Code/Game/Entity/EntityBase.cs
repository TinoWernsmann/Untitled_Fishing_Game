using UnityEngine;

using UnityEngine.InputSystem;
using Core.Game;
using System.Linq;
using System.Collections.Generic;
using System;


namespace Core.Game.Entity
{
    public class EntityBase :
    //MonoBehaviour,
    GameObjectBase, 
    IRaycastReceiver
    {
        protected Rigidbody rigidbodyPtr = null;
        protected Raycaster caster = new Raycaster();



        //auch debug testing
        public void ReceiveRaycast(UnityEngine.Vector3 origin)//override <=> new
        {
            //Debug.Log("EntityBase ReceiveRaycast" + origin + " self: " + gameObject.name);
            UnityEngine.Vector3 dir = origin - gameObject.transform.position; //AB = B - A
            dir *= -1;
            float force = 0.5f;
            ApplyImpulse(dir, force);
        }

        //debug testing 
        public virtual void ApplyImpulse(Vector3 forceDir, float v)
        {
            if (rigidbodyPtr)
            {
                //Debug.Log("EntityBase Apply velocity");
                Vector3 scale = forceDir.normalized * v;
                rigidbodyPtr.AddForce(scale, ForceMode.VelocityChange);
            }
        }

        public void RemoveVelocity()
        {
            if (rigidbodyPtr)
            {
                rigidbodyPtr.linearVelocity = Vector3.zero;
                rigidbodyPtr.angularVelocity = Vector3.zero;

            }
        }

        public void SetRigidBodyEnabled(bool flag)
        {
            if(rigidbodyPtr != null)
            {
                RemoveVelocity();
                rigidbodyPtr.isKinematic = true;
            }
            
        }

       
            



        protected virtual void Start()
        {
            FindRigidBody();
        }

        protected void FindRigidBody()
        {
            if (rigidbodyPtr == null)
            {
                rigidbodyPtr = GetComponent<Rigidbody>();
            }
            //add rigid body if needed
            if (rigidbodyPtr == null)
            {
                rigidbodyPtr = gameObject.AddComponent<Rigidbody>();
            }
        }



        void Update()
        {

        }


        public static Bounds GetGameObjectBounds(GameObject target)
        {
            // Get all renderers attached to the target and its children
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                // Fallback to transform position if no renderers exist
                return new Bounds(target.transform.position, Vector3.zero);
            }

            // Initialize bounds with the first renderer
            Bounds combinedBounds = renderers[0].bounds;

            // Encapsulate the rest of the renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            return combinedBounds;
        }   



        public void LookAt2D(GameObject other)
        {
            if(other != null)
            {
                LookAt2D(other.transform.position);
            }
        }
        
        public void LookAt2D(Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0f; // Y-Achse ignorieren

            if (dir.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

    };   
}