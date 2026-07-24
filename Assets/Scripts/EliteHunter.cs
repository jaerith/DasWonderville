using UnityEngine;

public class EliteHunter : MonoBehaviour
{
    private void Awake()
    {
        EnemyHunterBehavior hunterBehavior = GetComponent<EnemyHunterBehavior>();
        if (hunterBehavior == null)
            return;

        hunterBehavior.FireIntervalSeconds *= 2f / 3f;
        hunterBehavior.PursuitSpeed *= 4f / 3f;
    }
}
