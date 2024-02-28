using UnityEngine;

[RequireComponent(typeof(IDamageable))]
public class SpawnParticleSystemOnDeath : MonoBehaviour
{
    [SerializeField] private ParticleSystem DeathSystem;
    public IDamageable Damageable;

    private void Awake()
    {
        Damageable = GetComponent<IDamageable>();
    }

    private void OnEnable()
    {
        Damageable.OnDeath += SpawnParticleSystem;
    }

    private void SpawnParticleSystem(Vector3 Position)
    {
        gameObject.SetActive(false);
        Instantiate(DeathSystem, Position, Quaternion.identity);
    }
}
