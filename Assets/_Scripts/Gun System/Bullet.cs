using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody Rigidbody;
    [field: SerializeField]

    public Vector3 SpawnLocation
    {
        get;
        private set;
    }

    [SerializeField] private float DelayedDisableTime = 2f;

    public delegate void CollisionEvent(Bullet Bullet, Collision Collision);
    public event CollisionEvent OnCollision;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void Spawn(Vector3 SpawnForce)
    {
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        Rigidbody.AddForce(SpawnForce);
        StartCoroutine(DelayedDisable(DelayedDisableTime));
    }

    private IEnumerator DelayedDisable(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        OnCollisionEnter(null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision?.Invoke(this, collision);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;
    }
}
