using UnityEngine;

public class TorpedoMover : MonoBehaviour
{
    private Vector3 direction;
    private Vector3 startPosition;
    private float speed;
    private float maxDistance;

    public void Initialize(Vector3 moveDirection, float moveSpeed, float destroyDistance)
    {
        direction = moveDirection;
        direction.y = 0f;
        direction.Normalize();

        speed = moveSpeed;
        maxDistance = destroyDistance;
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        Vector3 flatOffset = transform.position - startPosition;
        flatOffset.y = 0f;

        if (flatOffset.magnitude >= maxDistance)
            Destroy(gameObject);
    }
}