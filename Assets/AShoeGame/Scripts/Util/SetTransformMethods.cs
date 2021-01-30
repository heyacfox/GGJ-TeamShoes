using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple helper script that exposes public methods for changing transforms (for unity event callbacks, since you can't set properties of transform this way)
public class SetTransformMethods : MonoBehaviour
{
    public void ResetToAwakeWorldPosition() { SetWorldPosition(awakeWorldPos); }
    public void ResetToAwakeLocalPosition() { SetLocalPosition(awakeLocalPos); }

    public void ResetToAwakeWorldRotation() { SetWorldRotation(awakeWorldRot); }
    public void ResetToAwakeLocalRotation() { SetLocalRotation(awakeLocalRot); }

    public void ResetToAwakeLocalScale() { SetLocalScale(awakeLocalScale); }


    public void CopyWorldPosition(Transform other) { SetWorldPosition(other.position); }
    public void CopyLocalPosition(Transform other) { SetLocalPosition(other.localPosition); }

    public void CopyWorldRotation(Transform other) { SetWorldRotation(other.rotation.eulerAngles); }
    public void CopyLocalRotation(Transform other) { SetLocalRotation(other.localRotation.eulerAngles); }

    public void CopyLocalScale(Transform other) { SetLocalScale(other.localScale); }


    public void SetWorldPositionX(float xPos) { SetWorldPosition(new Vector3(xPos, transform.position.y, transform.position.z)); }
    public void SetWorldPositionY(float yPos) { SetWorldPosition(new Vector3(transform.position.x, yPos, transform.position.z)); }
    public void SetWorldPositionZ(float zPos) { SetWorldPosition(new Vector3(transform.position.x, transform.position.y, zPos)); }

    public void SetLocalPositionX(float xPos) { SetLocalPosition(new Vector3(xPos, transform.localPosition.y, transform.localPosition.z)); }
    public void SetLocalPositionY(float yPos) { SetLocalPosition(new Vector3(transform.localPosition.x, yPos, transform.localPosition.z)); }
    public void SetLocalPositionZ(float zPos) { SetLocalPosition(new Vector3(transform.localPosition.x, transform.localPosition.y, zPos)); }

    public void SetWorldRotationX(float xPos) { SetWorldRotation(new Vector3(xPos, transform.rotation.y, transform.rotation.z)); }
    public void SetWorldRotationY(float yPos) { SetWorldRotation(new Vector3(transform.rotation.x, yPos, transform.rotation.z)); }
    public void SetWorldRotationZ(float zPos) { SetWorldRotation(new Vector3(transform.rotation.x, transform.rotation.y, zPos)); }

    public void SetLocalRotationX(float xPos) { SetLocalRotation(new Vector3(xPos, transform.localRotation.y, transform.localRotation.z)); }
    public void SetLocalRotationY(float yPos) { SetLocalRotation(new Vector3(transform.localRotation.x, yPos, transform.localRotation.z)); }
    public void SetLocalRotationZ(float zPos) { SetLocalRotation(new Vector3(transform.localRotation.x, transform.localRotation.y, zPos)); }

    public void SetLocalScaleX(float xPos) { SetLocalScale(new Vector3(xPos, transform.localScale.y, transform.localScale.z)); }
    public void SetLocalScaleY(float yPos) { SetLocalScale(new Vector3(transform.localScale.x, yPos, transform.localScale.z)); }
    public void SetLocalScaleZ(float zPos) { SetLocalScale(new Vector3(transform.localScale.x, transform.localScale.y, zPos)); }


    public void SetWorldPosition(Vector3 worldPos) { transform.position = worldPos; }
    public void SetLocalPosition(Vector3 localPos) { transform.localPosition = localPos; }

    public void SetWorldRotation(Vector3 eulerRotation) { transform.rotation = Quaternion.Euler(eulerRotation); }
    public void SetLocalRotation(Vector3 eulerRotation) { transform.localRotation = Quaternion.Euler(eulerRotation); }

    public void SetLocalScale(Vector3 localScale) { transform.localScale = localScale; }


    Vector3 awakeLocalScale, awakeLocalPos, awakeWorldPos, awakeLocalRot, awakeWorldRot;

    private void Awake()
    {
        awakeWorldPos = transform.position;
        awakeLocalPos = transform.localPosition;
        awakeWorldRot = transform.rotation.eulerAngles;
        awakeLocalRot = transform.localRotation.eulerAngles;
        awakeLocalScale = transform.localScale;
    }
}
