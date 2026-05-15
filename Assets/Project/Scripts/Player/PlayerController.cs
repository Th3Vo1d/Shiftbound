using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Ortak Karakter Verileri")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 5f;

    private float currentHealth;
    private Renderer characterRenderer;
    private MaterialPropertyBlock propertyBlock;

    // --- State Instances ---
    private MeleeState meleeState;
    private ArcherState archerState;
    private WizardState wizardState;

    // --- New Input System Actions ---
    private InputAction switchMeleeAction;
    private InputAction switchArcherAction;
    private InputAction switchWizardAction;

    // --- Public Properties ---
    public IPlayerState CurrentState { get; private set; }
    public Renderer CharacterRenderer => characterRenderer;
    public MaterialPropertyBlock PropertyBlock => propertyBlock;
    public float MaxHealth => maxHealth;

    public float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0f, maxHealth);
    }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        propertyBlock = new MaterialPropertyBlock();

        // MeshRenderer veya SkinnedMeshRenderer'ı bul (children dahil)
        characterRenderer = GetComponentInChildren<Renderer>();

        if (characterRenderer == null)
        {
            Debug.LogWarning("[PlayerController] Renderer bulunamadı! Renk değişimi çalışmayacak.");
        }

        // State'leri oluştur
        meleeState = new MeleeState();
        archerState = new ArcherState();
        wizardState = new WizardState();

        // Input Action'ları programatik olarak tanımla (New Input System)
        switchMeleeAction = new InputAction("SwitchMelee", InputActionType.Button, "<Keyboard>/1");
        switchArcherAction = new InputAction("SwitchArcher", InputActionType.Button, "<Keyboard>/2");
        switchWizardAction = new InputAction("SwitchWizard", InputActionType.Button, "<Keyboard>/3");

        // Performed event'lerine state geçişlerini bağla
        switchMeleeAction.performed += _ => ChangeState(meleeState);
        switchArcherAction.performed += _ => ChangeState(archerState);
        switchWizardAction.performed += _ => ChangeState(wizardState);
    }

    private void OnEnable()
    {
        switchMeleeAction?.Enable();
        switchArcherAction?.Enable();
        switchWizardAction?.Enable();
    }

    private void OnDisable()
    {
        switchMeleeAction?.Disable();
        switchArcherAction?.Disable();
        switchWizardAction?.Disable();
    }

    private void Start()
    {
        // Varsayılan olarak MeleeState ile başla
        ChangeState(meleeState);
    }

    private void Update()
    {
        CurrentState?.Update(this);
    }

    /// <summary>
    /// Mevcut state'den çıkıp yeni state'e geçiş yapar.
    /// </summary>
    /// <param name="newState">Geçiş yapılacak yeni state.</param>
    public void ChangeState(IPlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("[PlayerController] Yeni state null olamaz!");
            return;
        }

        // Aynı state'e tekrar geçişi engelle
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
    /// Karakterin rengini MaterialPropertyBlock ile değiştirir.
    /// Orijinal materyali klonlamaz, performans dostudur.
    /// </summary>
    public void SetCharacterColor(Color color)
    {
        if (characterRenderer == null) return;

        propertyBlock.SetColor("_BaseColor", color); // URP/HDRP
        propertyBlock.SetColor("_Color", color);     // Built-in RP fallback
        characterRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDestroy()
    {
        // Input Action'ları temizle
        switchMeleeAction?.Dispose();
        switchArcherAction?.Dispose();
        switchWizardAction?.Dispose();
    }
}
