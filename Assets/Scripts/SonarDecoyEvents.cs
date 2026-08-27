using UnityEngine;

public class SonarDecoyEvents : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string torpedoTag = "Torpedo";
    [SerializeField] private string decoyTag = "Decoy";

    [Header("Detection")]
    [SerializeField] private float decoyDetectionRadius = 5f;

    [Header("Decoy Effect")]
    [SerializeField] private float decoyLateralDriftSpeed = 1.5f;
    [SerializeField] private float leftDriftDirection = -0.5f;
    [SerializeField] private float rightDriftDirection = 0.5f;

    private void Update()
    {
        GameObject[] torpedoes = GameObject.FindGameObjectsWithTag(torpedoTag);
        GameObject[] decoys = GameObject.FindGameObjectsWithTag(decoyTag);

        foreach (GameObject torpedoObj in torpedoes)
        {
            TorpedoMover mover = torpedoObj.GetComponent<TorpedoMover>();
            if (mover == null || mover.AffectedByDecoy)
                continue;

            foreach (GameObject decoyObj in decoys)
            {
                float dist = Vector3.Distance(torpedoObj.transform.position, decoyObj.transform.position);
                if (dist > decoyDetectionRadius)
                    continue;

                ApplyDecoyEffect(mover, torpedoObj.transform, decoyObj.transform);
                break;
            }
        }
    }

    private void ApplyDecoyEffect(TorpedoMover mover, Transform torpedo, Transform decoy)
    {
        Vector3 lateralDir = Vector3.Cross(Vector3.up, mover.Direction);
        Vector3 toDecoy = decoy.position - torpedo.position;

        bool isOnRight = Vector3.Dot(toDecoy, lateralDir) > 0f;

        mover.LateralDriftSpeed = decoyLateralDriftSpeed;
        mover.DriftDirection = isOnRight ? rightDriftDirection : leftDriftDirection;
        mover.AffectedByDecoy = true;
    }
}
