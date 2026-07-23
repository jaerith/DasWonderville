using UnityEngine;

public class EnemyShipLeader : MonoBehaviour
{
    [SerializeField] private string shipTag = "Ship";

    public void InvokeLeaderSinkEffects()
    {
        Debug.Log("Leading is sinking - slowing down all hunters.");

        GameObject[] ships = GameObject.FindGameObjectsWithTag(shipTag);

        foreach (GameObject ship in ships)
        {
            if (!ship.activeInHierarchy)
                continue;

            EnemyHunterBehavior hunterBehavior = ship.GetComponent<EnemyHunterBehavior>();
            if (hunterBehavior == null || hunterBehavior.isSinking)
            {
                //Debug.Log("Hunter(" + ship.name + ") could not be set to half speed because " +
                //          ((hunterBehavior == null) ? "no behavior is found" : "target ship is sinking"));

                continue;
            }

            Debug.Log("Hunter(" + ship.name + ") set to half speed.");

            hunterBehavior.PursuitSpeed *= 0.5f;
            hunterBehavior.RotationSpeed *= 0.5f;

            Debug.Log("Hunter(" + ship.name + ") set to half speed.");
            Debug.Log("Hunter(" + ship.name + ") set to half speed.");
        }
    }
}
