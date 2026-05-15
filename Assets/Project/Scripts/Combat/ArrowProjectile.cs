using UnityEngine;

/// <summary>
/// Okçu state'inin fırlattığı ok mermisi.
/// İleri yönde sabit hızla hareket eder, düşmana çarpınca hasar verir ve yok olur.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxRange = 25f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        // Menzile ulaşınca yok ol (25 birim / 20 hız = 1.25 saniye)
        float lifetime = maxRange / speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Player'a çarpmasın
        if (other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log($"[ArrowProjectile] {other.name} hedefine {damage} hasar verildi!");
        }

        Destroy(gameObject);
    }
}
