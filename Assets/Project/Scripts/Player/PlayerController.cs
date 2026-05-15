using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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

    // --- State Instances ---
    private MeleeState meleeState;
    private ArcherState archerState;
    private WizardState wizardState;

    // --- Input Actions ---
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction switchMeleeAction;
    private InputAction switchArcherAction;
    private InputAction switchWizardAction;

    private Vector2 moveInput;

    // --- Public Properties ---
    public IPlayerState CurrentState { get; private set; }
    public Renderer CharacterRenderer => characterRenderer;
    public MaterialPropertyBlock PropertyBlock => propertyBlock;
    public Transform AttackPoint => attackPoint;
    public GameObject ArrowPrefab => arrowPrefab;
    public LayerMask EnemyLayer => enemyLayer;
    public float MaxHealth => maxHealth;

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
            Debug.LogWarning("[PlayerController] Renderer bulunamadı!");
        if (rb == null)
            Debug.LogWarning("[PlayerController] Rigidbody bulunamadı!");

        // State'leri oluştur
        meleeState = new MeleeState();
        archerState = new ArcherState();
        wizardState = new WizardState();

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

        switchMeleeAction = new InputAction("SwitchMelee", InputActionType.Button, "<Keyboard>/1");
        switchArcherAction = new InputAction("SwitchArcher", InputActionType.Button, "<Keyboard>/2");
        switchWizardAction = new InputAction("SwitchWizard", InputActionType.Button, "<Keyboard>/3");

        // --- Event Bağlantıları ---
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += _ => moveInput = Vector2.zero;
        attackAction.performed += _ => CurrentState?.OnAttack(this);
        switchMeleeAction.performed += _ => ChangeState(meleeState);
        switchArcherAction.performed += _ => ChangeState(archerState);
        switchWizardAction.performed += _ => ChangeState(wizardState);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        attackAction?.Enable();
        switchMeleeAction?.Enable();
        switchArcherAction?.Enable();
        switchWizardAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        attackAction?.Disable();
        switchMeleeAction?.Disable();
        switchArcherAction?.Disable();
        switchWizardAction?.Disable();
    }

    private void Start()
    {
        ChangeState(meleeState);
    }

    private void Update()
    {
        CurrentState?.Update(this);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// WASD ile XZ düzleminde hareket. Hız değeri aktif State'den alınır.
    /// </summary>
    private void HandleMovement()
    {
        if (rb == null || CurrentState == null) return;

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Vector3 velocity = movement * CurrentState.MoveSpeed;
        velocity.y = rb.linearVelocity.y; // Yerçekimini koru
        rb.linearVelocity = velocity;

        // Hareket yönüne doğru dön
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    public void ChangeState(IPlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("[PlayerController] Yeni state null olamaz!");
            return;
        }

        if (CurrentState == newState)
        {
            Debug.Log($"[PlayerController] Zaten {CurrentState.GetType().Name} state'inde!");
            return;
        }

        Debug.Log($"[PlayerController] State değişimi: {CurrentState?.GetType().Name ?? "None"} → {newState.GetType().Name}");

        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
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
        switchMeleeAction?.Dispose();
        switchArcherAction?.Dispose();
        switchWizardAction?.Dispose();
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
