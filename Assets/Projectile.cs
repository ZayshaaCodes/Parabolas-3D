using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    public float shootVelocity = 8;
    public float moveSpeed = 5;
    public GameObject spawnOnDeath;
    public float explosionForce = 9;
    public float explosionRadius = 2;
    public LayerMask mask;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_rb.velocity != Vector2.zero)
        {
            transform.rotation = Quaternion.LookRotation(_rb.velocity);
            
        }

        if (_rb.velocity.magnitude > moveSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * moveSpeed;
        }
    }

    private void OnCollisionEnter2D()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, mask);

        foreach (var coll in colliders)
        {
            var delta = coll.transform.position - transform.position;
            if (coll.attachedRigidbody is {} rb) 
                rb.AddForce(delta.normalized * explosionForce, ForceMode2D.Impulse);
        }
        
        var explosion = Instantiate(spawnOnDeath, transform.position, Quaternion.identity);
        Destroy(explosion, explosion.GetComponent<Animation>().clip.length);
        Destroy(gameObject);
    }

    public void Shoot(Vector3 direction)
    {
        _rb.velocity = direction * shootVelocity;
    }
}