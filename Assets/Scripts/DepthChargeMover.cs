using UnityEngine;
using UnityEngine.Events;

public class DepthChargeMover : MonoBehaviour
{
    [Header("Sink Settings")]
    [SerializeField] private float sinkSpeed = 1.5f;
    [SerializeField] private float maxDepth = 50f;

    [SerializeField] UnityEvent onDepthChargeAwakened;
    [SerializeField] UnityEvent onMaxDepthReached;

    private float startY;

    public float SinkSpeed
    {
        get => sinkSpeed;
        set => sinkSpeed = value;
    }

    public float MaxDepth
    {
        get => maxDepth;
        set => maxDepth = value;
    }

    public void Initialize(float moveSpeed, float destroyDepth)
    {
        sinkSpeed = moveSpeed;
        maxDepth = destroyDepth;
    }

    private void Awake()
    {
        startY = transform.position.y;
        // onDepthChargeAwakened.Invoke();
    }

    private void Start()
    {
    }

    private void Update()
    {
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;

        float depthTraveled = startY - transform.position.y;

        if (depthTraveled >= maxDepth)
        {
            onMaxDepthReached.Invoke();
            Destroy(gameObject);
        }
    }
}
