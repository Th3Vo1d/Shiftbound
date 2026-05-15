using UnityEngine;

/// <summary>
/// Büyücü — AoE hasar state'i.
/// Düşük hız, yüksek AoE hasarı, mouse pozisyonunda patlama.
/// </summary>
public class WizardState : IPlayerState
{
    private readonly Color stateColor = new Color(0.55f, 0.24f, 0.88f);

    // --- Denge Değerleri ---
    private const float MOVE_SPEED = 4f;
    private const float AOE_DAMAGE = 40f;
    private const float AOE_RADIUS = 3f;
    private const float ATTACK_COOLDOWN = 1.2f;
    private const float MAX_CAST_RANGE = 15f;

    private float lastAttackTime = -999f;

    public float MoveSpeed => MOVE_SPEED;

    public void Enter(PlayerController controller)
    {
        Debug.Log("[WizardState] 🔮 Büyücü moduna geçildi.");
        Debug.Log($"[WizardState] Hız:{MOVE_SPEED} | AoE Hasar:{AOE_DAMAGE} | Yarıçap:{AOE_RADIUS} | Bekleme:{ATTACK_COOLDOWN}s");
        controller.SetCharacterColor(stateColor);
    }

    public void Update(PlayerController controller)
    {
        // TODO: Wizard'a özel Update mantığı (mana sistemi vb.)
    }

    public void Exit(PlayerController controller)
    {
        Debug.Log("[WizardState] Exit — Büyücü modundan çıkıldı.");
    }

    public void OnAttack(PlayerController controller)
    {
        if (Time.time - lastAttackTime < ATTACK_COOLDOWN) return;
        lastAttackTime = Time.time;

        Vector3 mousePos = controller.GetMouseWorldPosition();

        // Menzil kontrolü
        float distance = Vector3.Distance(controller.transform.position, mousePos);
        if (distance > MAX_CAST_RANGE)
        {
            Debug.Log("[WizardState] ❌ Hedef çok uzak! (Maks menzil: " + MAX_CAST_RANGE + ")");
            return;
        }

        Debug.Log($"[WizardState] 🔮 AoE patlama! Pozisyon: {mousePos}");

        Collider[] hits = Physics.OverlapSphere(mousePos, AOE_RADIUS, controller.EnemyLayer);

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(AOE_DAMAGE);
                Debug.Log($"[WizardState] {hit.name} → {AOE_DAMAGE} AoE hasar!");
            }
        }

        if (hits.Length == 0)
            Debug.Log("[WizardState] AoE alanında düşman yok.");
    }
}
