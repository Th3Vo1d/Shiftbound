using UnityEngine;

/// <summary>
/// Aktif karakteri smooth lerp ile takip eden kamera kontrolcüsü.
/// CharacterManager tarafından hedef değiştirildiğinde yumuşak geçiş yapar.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSmoothSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 3f;

    [Header("Geçiş Ayarları")]
    [Tooltip("Karakter geçişi sırasında kullanılan daha hızlı lerp değeri")]
    [SerializeField] private float switchSmoothSpeed = 3f;
    [SerializeField] private float switchDuration = 0.8f;

    private Transform target;
    private float switchTimer;
    private bool isSwitching;

    /// <summary>
    /// Kameranın takip edeceği hedefi değiştirir.
    /// Smooth geçiş otomatik olarak başlar.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        if (target != null && target != newTarget)
        {
            // Yeni bir hedefe geçiş başlat
            isSwitching = true;
            switchTimer = switchDuration;
        }
        else if (target == null)
        {
            // İlk hedef — anında pozisyona git
            transform.position = newTarget.position + offset;
            transform.LookAt(newTarget.position);
        }

        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Geçiş zamanlayıcısını güncelle
        if (isSwitching)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
            {
                isSwitching = false;
            }
        }

        // Aktif smoothing hızını seç
        float currentSmoothSpeed = isSwitching ? switchSmoothSpeed : followSmoothSpeed;

        // Hedef pozisyon
        Vector3 desiredPosition = target.position + offset;

        // Smooth pozisyon takibi
        transform.position = Vector3.Lerp(transform.position, desiredPosition, currentSmoothSpeed * Time.deltaTime);

        // Smooth rotasyon — hedefe bak
        Vector3 lookDirection = target.position - transform.position;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
