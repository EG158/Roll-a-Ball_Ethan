/************************************************************
* The script id attached to the Main Camera
* COMPONENT OF: Camera
* Purpose: controlling the movement of camera
* AUTHOR: Ethan Guo
* Date written: Fri, Sep 18
* VERSION: 1.0
*************************************************************/

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = playerTransform.position + offset;
    
    }
}
