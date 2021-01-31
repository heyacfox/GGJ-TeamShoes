using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public List<Transform> targetCharacters;
    public Vector3 targetCorrection = new Vector3(0, 0.9f, 0f);

    public Camera usedCamera;

    public float stepFieldOfView = 0.5f;
    public float rotationSpeed = 10f;
    public float maxRotationSpeed = 120f;
    private float maxRotationHeight;
    public float maxTargetCorrection = 2f;

    private bool circulateGroup = false;
    private float cameraAngle;
    private float cameraDistance;

    private void Start()
    {
        cameraAngle = (float)(Mathf.Atan2(usedCamera.transform.localPosition.x, usedCamera.transform.localPosition.y));
        cameraDistance = Vector3.Distance(usedCamera.transform.position, transform.position);
        maxRotationHeight = maxRotationSpeed / 180 * Mathf.PI;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = new Vector3();
        foreach (Transform character in targetCharacters)
        {
            targetPosition += character.position;
        }
        targetPosition = targetPosition / targetCharacters.Count + targetCorrection;
        this.transform.position = targetPosition;

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            usedCamera.fieldOfView += stepFieldOfView;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            usedCamera.fieldOfView -= stepFieldOfView;
        }

        if (Input.GetMouseButton(0))
        {
            circulateGroup = false;
            Vector3 mousePosition = Input.mousePosition;
            float relativeX = (  mousePosition.x-Screen.width / 2) / Screen.width / 2;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetCorrection.x += maxTargetCorrection * relativeX * Time.deltaTime;
            }
            else
            {
                transform.Rotate(0, relativeX * maxRotationSpeed * Time.deltaTime, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            circulateGroup = !circulateGroup;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 mousePosition = Input.mousePosition;
            float relativeY = (Screen.height / 2 - mousePosition.y) / Screen.height / -2;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                targetCorrection.y += maxTargetCorrection *relativeY* Time.deltaTime;
            }
            else
            {
                cameraAngle += relativeY * maxRotationHeight * Time.deltaTime;
                float newx = Mathf.Sin(cameraAngle) * cameraDistance;
                float newy = Mathf.Cos(cameraAngle) * cameraDistance;
                usedCamera.transform.localPosition = new Vector3(newx, newy, 0);
            }
        }

        if (circulateGroup)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        usedCamera.transform.LookAt(targetPosition);
    }
}
