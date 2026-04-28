using UnityEngine;

[ExecuteAlways]
public class PeriscopeHudFollower : MonoBehaviour
{
    public Transform targetCamera;
    public float distanceFromCamera = 1.15f;
    public Vector3 localOffset = new Vector3(0f, -0.02f, 0f);

    private void LateUpdate()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        if (targetCamera == null)
            return;

        transform.position =
            targetCamera.position +
            targetCamera.forward * distanceFromCamera +
            targetCamera.TransformVector(localOffset);

        transform.rotation = targetCamera.rotation;
    }
}