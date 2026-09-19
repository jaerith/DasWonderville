using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MonsterBehavior : MonoBehaviour
{
    private const string ShipTag = "Ship";

    [SerializeField] private AudioClip monsterSound;

    public void Start()
    {
        if (monsterSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = monsterSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(ShipTag))
            return;

        // The collider may live on a child of the ship, so search up the hierarchy.
        ShipHitReaction hitReaction = other.GetComponentInParent<ShipHitReaction>();

        if (hitReaction == null)
        {
            Debug.LogWarning("MonsterBehavior: (" + other.gameObject.name + ") is tagged Ship but has no ShipHitReaction.");
            return;
        }

        Debug.Log("MonsterBehavior: destroying ship (" + hitReaction.gameObject.name + ").");
        hitReaction.DestroyShip();
    }
}
