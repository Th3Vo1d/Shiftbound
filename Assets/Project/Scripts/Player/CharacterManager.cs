using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3 karakter arasında geçişi yöneten ana yönetici.
/// Sahnedeki 3 karakteri (Melee, Archer, Wizard) kontrol eder.
/// 1-2-3 tuşlarıyla geçiş, 10 saniyelik cooldown.
/// Pasif karakterler CompanionAI ile aktif karakteri takip eder.
/// Kamera smooth lerp ile aktif karakteri izler.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [Header("Karakter Referansları")]
    [Tooltip("Sahnedeki Melee karakter GameObject'i")]
    [SerializeField] private PlayerController meleeCharacter;
    [Tooltip("Sahnedeki Archer karakter GameObject'i")]
    [SerializeField] private PlayerController archerCharacter;
    [Tooltip("Sahnedeki Wizard karakter GameObject'i")]
    [SerializeField] private PlayerController wizardCharacter;

    [Header("Kamera")]
    [SerializeField] private CameraController cameraController;

    [Header("Geçiş Ayarları")]
    [SerializeField] private float switchCooldown = 10f;

    [Header("Başlangıç Karakteri")]
    [SerializeField] private CharacterType startingCharacter = CharacterType.Melee;

    // --- Input Actions ---
    private InputAction switchMeleeAction;
    private InputAction switchArcherAction;
    private InputAction switchWizardAction;

    // --- İç Durum ---
    private PlayerController activeCharacter;
    private float lastSwitchTime = -999f;
    private PlayerController[] allCharacters;

    // --- Public Properties ---
    public PlayerController ActiveCharacter => activeCharacter;
    public float SwitchCooldown => switchCooldown;

    /// <summary>
    /// Geçiş için kalan bekleme süresi (0 = geçiş yapılabilir).
    /// </summary>
    public float RemainingCooldown
    {
        get
        {
            float elapsed = Time.time - lastSwitchTime;
            float remaining = switchCooldown - elapsed;
            return remaining > 0f ? remaining : 0f;
        }
    }

    /// <summary>
    /// Geçiş yapılabilir mi?
    /// </summary>
    public bool CanSwitch => RemainingCooldown <= 0f;

    private void Awake()
    {
        // Karakter referanslarını doğrula
        if (meleeCharacter == null || archerCharacter == null || wizardCharacter == null)
        {
            Debug.LogError("[CharacterManager] Tüm karakter referansları atanmalı!");
            return;
        }

        allCharacters = new PlayerController[] { meleeCharacter, archerCharacter, wizardCharacter };

        // Kamera referansını otomatik bul (atanmamışsa)
        if (cameraController == null)
        {
            cameraController = Camera.main?.GetComponent<CameraController>();
            if (cameraController == null)
                Debug.LogWarning("[CharacterManager] CameraController bulunamadı! Ana kameraya CameraController scripti ekleyin.");
        }

        // --- Geçiş Input Actions ---
        switchMeleeAction = new InputAction("SwitchMelee", InputActionType.Button, "<Keyboard>/1");
        switchArcherAction = new InputAction("SwitchArcher", InputActionType.Button, "<Keyboard>/2");
        switchWizardAction = new InputAction("SwitchWizard", InputActionType.Button, "<Keyboard>/3");

        switchMeleeAction.performed += _ => SwitchTo(meleeCharacter);
        switchArcherAction.performed += _ => SwitchTo(archerCharacter);
        switchWizardAction.performed += _ => SwitchTo(wizardCharacter);
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
        // Başlangıç karakterini belirle
        PlayerController startChar = startingCharacter switch
        {
            CharacterType.Melee  => meleeCharacter,
            CharacterType.Archer => archerCharacter,
            CharacterType.Wizard => wizardCharacter,
            _ => meleeCharacter
        };

        // Tüm karakterleri pasif yap
        foreach (var character in allCharacters)
        {
            character.SetActive(false);
        }

        // Başlangıç karakterini aktif yap (cooldown olmadan)
        ActivateCharacter(startChar);
        // İlk geçişte cooldown uygulanmasın
        lastSwitchTime = -999f;
    }

    /// <summary>
    /// Belirtilen karaktere geçiş yapar. Cooldown kontrolü içerir.
    /// </summary>
    public void SwitchTo(PlayerController newCharacter)
    {
        if (newCharacter == null) return;

        // Zaten aktif olan karaktere geçiş yapma
        if (newCharacter == activeCharacter)
        {
            Debug.Log($"[CharacterManager] {newCharacter.CharacterType} zaten aktif!");
            return;
        }

        // Cooldown kontrolü
        if (!CanSwitch)
        {
            Debug.Log($"[CharacterManager] Geçiş bekleme süresi: {RemainingCooldown:F1}s kaldı.");
            return;
        }

        Debug.Log($"[CharacterManager] ━━━ Karakter Geçişi: {activeCharacter?.CharacterType} → {newCharacter.CharacterType} ━━━");

        // Mevcut aktif karakteri pasif yap
        if (activeCharacter != null)
        {
            activeCharacter.SetActive(false);
            // CompanionAI'ı başlat
            CompanionAI companionAI = activeCharacter.GetComponent<CompanionAI>();
            if (companionAI != null)
            {
                companionAI.StartFollowing(newCharacter.transform);
            }
        }

        // Yeni karakteri aktif yap
        ActivateCharacter(newCharacter);

        // Cooldown zamanlayıcısını başlat
        lastSwitchTime = Time.time;
    }

    /// <summary>
    /// Karakteri aktif yapar ve diğerlerinin CompanionAI'larını günceller.
    /// </summary>
    private void ActivateCharacter(PlayerController character)
    {
        activeCharacter = character;
        activeCharacter.SetActive(true);

        // Aktif karakterin CompanionAI'ını durdur
        CompanionAI activeAI = activeCharacter.GetComponent<CompanionAI>();
        if (activeAI != null)
        {
            activeAI.StopFollowing();
        }

        // Diğer tüm pasif karakterlerin CompanionAI'larını yeni hedefe yönlendir
        foreach (var otherCharacter in allCharacters)
        {
            if (otherCharacter == activeCharacter) continue;

            CompanionAI companionAI = otherCharacter.GetComponent<CompanionAI>();
            if (companionAI != null)
            {
                companionAI.StartFollowing(activeCharacter.transform);
            }
        }

        // Kamerayı yeni hedefe yönlendir
        if (cameraController != null)
        {
            cameraController.SetTarget(activeCharacter.transform);
        }
    }

    private void OnDestroy()
    {
        switchMeleeAction?.Dispose();
        switchArcherAction?.Dispose();
        switchWizardAction?.Dispose();
    }
}
