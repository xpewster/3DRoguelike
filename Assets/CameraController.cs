using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    float mouseSens = 100f;

    private float mouseX, mouseY, dt;

    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        dt = Time.deltaTime;

        mouseX += Input.GetAxis("Mouse X") * mouseSens * dt;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSens * dt;

        mouseY = Mathf.Clamp(mouseY, -89, 89);

        playerTransform.rotation = Quaternion.Euler(0, mouseX, 0);
        transform.localRotation = Quaternion.Euler(mouseY, 0, 0);
    }
}
