using UnityEngine;

/// <summary>
/// Pasif (kontrol edilmeyen) karakterlerin aktif karakteri belirli bir mesafede
/// takip etmesini sağlayan basit AI scripti.
/// CharacterManager tarafından yönetilir.
/// </summary>
public class CompanionAI : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float stopDistance = 2.5f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Formasyon")]
    [Tooltip("Aktif karaktere göre offset pozisyonu (sağ/sol arkaya yerleşmek için)")]
    [SerializeField] private Vector3 followOffset = new Vector3(1.5f, 0f, -1.5f);

    private Transform target;
    private Rigidbody rb;
    private bool isFollowing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Takip edilecek hedefi ayarlar ve AI'ı aktif eder.
    /// </summary>
    public void StartFollowing(Transform newTarget)
    {
        target = newTarget;
        isFollowing = true;
    }

    /// <summary>
    /// AI takibini durdurur (karakter aktif olduğunda).
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
        target = null;

        // Hızı sıfırla
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (!isFollowing || target == null || rb == null) return;

        // Hedef pozisyon = aktif karakter + offset
        Vector3 targetPosition = target.position + target.TransformDirection(followOffset);
        targetPosition.y = transform.position.y; // Y eksenini koru

        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;

        // Çok uzaksa ışınlan
        if (distance > teleportDistance)
        {
            transform.position = targetPosition;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Yeterince yakınsa dur
        if (distance <= stopDistance)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

            // Hedefe doğru bak
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Vector3 lookDir = (target.position - transform.position);
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }
            }
            return;
        }

        // Hedefe doğru hareket et
        Vector3 direction = toTarget.normalized;
        Vector3 velocity = direction * followSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        // Hareket yönüne dön
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
