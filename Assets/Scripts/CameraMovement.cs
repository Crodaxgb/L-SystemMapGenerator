using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 This script mimics the editor camera movement
 
 */
public class CameraMovement : MonoBehaviour
{
    
        public float lookSpeedHorizontal = 2f;
        public float lookSpeedVertical = 2f;
        public float zoomSpeed = 2f;
        public float dragSpeed = 6f;

        private float yaw = 0f;
        private float pitch = 0f;

        void Update()
        {
            //Look around with Right Mouse
            if (Input.GetMouseButton(1))
            {
                yaw += lookSpeedHorizontal * Input.GetAxis("Mouse X");
                pitch -= lookSpeedVertical * Input.GetAxis("Mouse Y");

                transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }

            //drag camera around with Middle Mouse
            if (Input.GetMouseButton(2))
            {
                transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
            }

            //Zoom in and out with Mouse Wheel
            transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
        }
    }

