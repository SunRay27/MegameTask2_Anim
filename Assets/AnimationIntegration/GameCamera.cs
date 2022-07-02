using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public float distance = 15;
    public float angleY = 15;
    public float angleX = 15;

    public Transform target;

    private void Start()
    {
        UpdateCameraTransform();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        Vector3 targetPosition = target.position;
        transform.position = targetPosition + (Quaternion.Euler(angleX, angleY, 0) * Vector3.up) * distance;
        transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
    }

    public Vector3 GetForwardMoveDirectionVector()
    {
        return new Vector3(transform.forward.x, 0, transform.forward.z);
    }
}
