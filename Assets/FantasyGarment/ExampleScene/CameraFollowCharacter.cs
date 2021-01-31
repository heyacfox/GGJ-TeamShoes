using UnityEngine;
public class CameraFollowCharacter : MonoBehaviour
{
    public Transform playerObject1;
    public Transform playerObject2;
    public float distanceFromObject = 6f;
    public Vector3 playerMiddle = new Vector3(0, 1, 0);
    public float flyAroudSpeed;

    void Update()
    {
        Vector3 lookOnObject;
        Vector3 targetPosition;
        targetPosition = (playerObject1.position + playerObject2.position) / 2;
        lookOnObject = targetPosition + playerMiddle - transform.position;
        transform.forward = lookOnObject.normalized;
        Vector3 playerLastPosition;
        playerLastPosition = targetPosition + playerMiddle - lookOnObject.normalized * distanceFromObject;
        transform.position = playerLastPosition;
    }
}