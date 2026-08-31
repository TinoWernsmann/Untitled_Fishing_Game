



////WaterInteractionManager.Instance.UpdateTransformSurfacePositionFromRender(transform);

using UnityEngine;

using UnityEngine.InputSystem;
using Core.Game;
using System.Linq;
using System.Collections.Generic;
using System;




namespace Core.Game.Entity
{
    /// <summary>
    /// bringt leben ins spiel
    /// </summary>
    public class AmbientPlane : EntityBase
    {
        public static void Make(GameObject prefab, Vector3 center, float height)
        {
            if (prefab)
            {
                GameObject spawned = Instantiate<GameObject>(prefab);
                if (spawned){
                    AmbientPlane plane = spawned.GetComponent<AmbientPlane>();
                    if (plane)
                    {
                        plane.UpdateCenterAndHeight(center, height);
                    }
                }
            }
        }




        protected Vector3 center;


        private float theta = 0.0f;

        [Header("Movement")]
        [SerializeField] protected float height = 30;
        [SerializeField] protected float radius = 8;

        [SerializeField] protected float speed = 100;

        public GameObject propellor_0 = null;
        public GameObject propellor_1 = null;


        protected void UpdateCenterAndHeight(Vector3 other, float heightIn)
        {
            center = other;
            center.y = 0.0f;
            height = heightIn;
        }

        protected virtual void Update()
        {
            UpdateMovement();
            UpdatePropellors();
            PlaySound();
        }

        private void UpdateMovement()
        {
            //theta += speed * Time.deltaTime;
            theta = Mathf.Repeat(theta + speed * Time.deltaTime, 360f);

            // Convert theta (in degrees) into a Quaternion rotation around the Y-axis
            Quaternion currentRotation = Quaternion.AngleAxis(theta * -1.0f, Vector3.up);

            // Apply to object's transform


            Vector3 offset = new Vector3(radius, 0, 0);
            Vector3 offsetR = currentRotation * offset;

            //M = T * R <-- lese richtung --
            Vector3 M = center + offsetR + new Vector3(0, height, 0);


            SetRotation(currentRotation);
            SetLocation(M);

        }

        private void UpdatePropellors()
        {
            float propellorSpeed = 10000000;
            float angle = propellorSpeed * theta;
            UpdatePropellor(propellor_0, angle);
            UpdatePropellor(propellor_1, angle);

        }
        
        private void UpdatePropellor(GameObject other, float angle)
        {
            if (other)
            {
                Quaternion currentRotation = Quaternion.AngleAxis(angle, Vector3.up);
                other.transform.rotation = currentRotation;
            }
        }
        
        private void PlaySound()
        {
            //todo
        }




    }


}