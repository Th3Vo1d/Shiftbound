using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Karakter Tipi")]
    [SerializeField] private CharacterType characterType;

    [Header("Ortak Karakter Verileri")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Saldırı Referansları")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private LayerMask enemyLayer;

    private float currentHealth;
    private Renderer characterRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Rigidbody rb;
    private Camera mainCamera;

    // --- State Instance (her karakter tek bir state'e sahip) ---
    private IPlayerState characterState;

    // --- Input Actions ---
    private InputAction moveAction;
    private InputAction attackAction;

    private Vector2 moveInput;
    private bool isActive;

    // --- Public Properties ---
    public CharacterType CharacterType => characterType;
    public IPlayerState CurrentState => characterState;
    public bool IsActive => isActive;
    public Renderer CharacterRenderer => characterRenderer;
    public MaterialPropertyBlock PropertyBlock => propertyBlock;
    public Transform AttackPoint => attackPoint;
    public GameObject ArrowPrefab => arrowPrefab;
    public LayerMask EnemyLayer => enemyLayer;
    public float MaxHealth => maxHealth;
    public Rigidbody Rb => rb;

    public float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0f, maxHealth);
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        propertyBlock = new MaterialPropertyBlock();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        characterRenderer = GetComponentInChildren<Renderer>();

        if (characterRenderer == null)
            Debug.LogWarning($"[PlayerController:{characterType}] Renderer bulunamadı!");
        if (rb == null)
            Debug.LogWarning($"[PlayerController:{characterType}] Rigidbody bulunamadı!");

        // Karakter tipine göre tek bir state oluştur
        characterState = characterType switch
        {
            CharacterType.Melee  => new MeleeState(),
            CharacterType.Archer => new ArcherState(),
            CharacterType.Wizard => new WizardState(),
            _ => new MeleeState()
        };

        // --- Input Action Tanımları ---
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Keyboard>/space");
        attackAction.AddBinding("<Mouse>/leftButton");

        // --- Event Bağlantıları ---
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += _ => moveInput = Vector2.zero;
        attackAction.performed += _ => characterState?.OnAttack(this);
    }

    private void Start()
    {
        // State'e giriş yap
        characterState?.Enter(this);
    }

    private void Update()
    {
        if (!isActive) return;
        characterState?.Update(this);
    }

    private void FixedUpdate()
    {
        if (!isActive) return;
        HandleMovement();
    }

    /// <summary>
    /// Karakteri aktif/pasif yapar. Pasif karakter input almaz.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        if (active)
        {
            moveAction?.Enable();
            attackAction?.Enable();
            // State'e tekrar giriş yap (renk vs. güncelleme)
            characterState?.Enter(this);
            Debug.Log($"[PlayerController] {characterType} aktif edildi.");
        }
        else
        {
            moveAction?.Disable();
            attackAction?.Disable();
            moveInput = Vector2.zero;

            // Hareket durdur
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            Debug.Log($"[PlayerController] {characterType} pasif edildi.");
        }
    }

    /// <summary>
    /// WASD ile XZ düzleminde hareket. Hız değeri aktif State'den alınır.
    /// </summary>
    private void HandleMovement()
    {
        if (rb == null || characterState == null) return;

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 velocity = movement * characterState.MoveSpeed;
        velocity.y = rb.linearVelocity.y; // Yerçekimini koru
        rb.linearVelocity = velocity;

        // Hareket yönüne doğru dön
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// MaterialPropertyBlock ile renk değiştirir (materyali klonlamaz).
    /// </summary>
    public void SetCharacterColor(Color color)
    {
        if (characterRenderer == null) return;
        propertyBlock.SetColor("_BaseColor", color);
        propertyBlock.SetColor("_Color", color);
        characterRenderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// Mouse pozisyonunu Y=0 düzleminde world space'e çevirir.
    /// </summary>
    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return transform.position;
    }

    /// <summary>
    /// Karakterden mouse pozisyonuna doğru normalize edilmiş yön vektörü.
    /// </summary>
    public Vector3 GetDirectionToMouse()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 direction = mousePos - transform.position;
        direction.y = 0f;
        return direction.normalized;
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        attackAction?.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 1.5f);
        }
    }
}
