using System.Collections;
using UnityEngine;

public class TemporaryObjectFactory : MonoBehaviour
{
    [Header("Spawn Cycle")]
    [SerializeField] private GameObject objectToSpawn;

    [Tooltip("Each cycle waits a random time between 0 and this many seconds before spawning.")]
    [SerializeField] private float maxWaitSeconds = 2.0f;

    [Tooltip("How long each spawned copy exists before it is destroyed.")]
    [SerializeField] private float lifetimeSeconds = 20.0f;

    public float MaxWaitSeconds
    {
        get => maxWaitSeconds;
        set => maxWaitSeconds = value;
    }

    public float LifetimeSeconds
    {
        get => lifetimeSeconds;
        set => lifetimeSeconds = value;
    }

    public Coroutine CycleSpawn()
    {
        if (objectToSpawn == null)
        {
            Debug.LogWarning("TemporaryObjectFactory: CycleSpawn called with a null game object.");
            return null;
        }

        return StartCoroutine(SpawnCycleRoutine(objectToSpawn));
    }

    private IEnumerator SpawnCycleRoutine(GameObject providedGameObject)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0f, Mathf.Max(0f, maxWaitSeconds)));

            GameObject spawned = Instantiate(providedGameObject, transform.position, transform.rotation);
            spawned.SetActive(true);

            yield return new WaitForSeconds(Mathf.Max(0f, lifetimeSeconds));

            // Unity treats Destroy(null) / an already-destroyed object as a no-op,
            // so this is safe if the copy was destroyed by something else first.
            Destroy(spawned);
        }
    }
}
