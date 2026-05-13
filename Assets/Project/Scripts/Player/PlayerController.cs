using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ortak Karakter Verileri")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 5f;

    private float currentHealth;

    public IPlayerState CurrentState { get; private set; }

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
    }

    private void Update()
    {
        CurrentState?.Update(this);
    }

    /// <param name="newState">Geçiş yapılacak yeni state.</param>
    public void ChangeState(IPlayerState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning("[PlayerController] Yeni state null olamaz!");
            return;
        }

        Debug.Log($"[PlayerController] State değişimi: {CurrentState?.GetType().Name ?? "None"} → {newState.GetType().Name}");

        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }
}
