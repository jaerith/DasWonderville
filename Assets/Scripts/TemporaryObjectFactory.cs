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

    [Tooltip("Spawned objects are aimed at this transform (usually the Main Camera).")]
    [SerializeField] private Transform player;

    [Tooltip("Use this if the spawned model's front is not aligned with local +Z (e.g. 0, 90, 0 or 0, -90, 0).")]
    [SerializeField] private Vector3 aimRotationOffsetEuler = Vector3.zero;

    public Transform Player
    {
        get => player;
        set => player = value;
    }

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

            if (player == null)
            {
                Debug.LogWarning("TemporaryObjectFactory: Player is not assigned, so (" + spawned.name + ") was not aimed.");
            }
            else
            {
                spawned.transform.LookAt(player);

                // Strip pitch/roll so the spawn heading is level (yaw only).
                // Otherwise, aiming at a player above/below the spawn point tilts
                // transform.forward, and ShipMover only ever yaws afterward, so it
                // would climb/dive forever along that initial tilt.
                Vector3 levelEuler = spawned.transform.rotation.eulerAngles;
                levelEuler.x = 0f;
                levelEuler.z = 0f;
                spawned.transform.rotation = Quaternion.Euler(levelEuler);

                spawned.transform.rotation *= Quaternion.Euler(aimRotationOffsetEuler);
            }

            yield return new WaitForSeconds(Mathf.Max(0f, lifetimeSeconds));

            // Unity treats Destroy(null) / an already-destroyed object as a no-op,
            // so this is safe if the copy was destroyed by something else first.
            Destroy(spawned);
        }
    }
}
