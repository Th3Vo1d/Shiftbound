using UnityEngine;

/// <summary>
/// Savaşçı — Yakın dövüş state'i.
/// Orta hız, yüksek tek hedef hasarı, OverlapSphere saldırısı.
/// </summary>
public class MeleeState : IPlayerState
{
    private readonly Color stateColor = new Color(0.93f, 0.25f, 0.20f);

    // --- Denge Değerleri ---
    private const float MOVE_SPEED = 5.5f;
    private const float ATTACK_DAMAGE = 30f;
    private const float ATTACK_RANGE = 1.5f;
    private const float ATTACK_COOLDOWN = 0.5f;

    private float lastAttackTime = -999f;

    public float MoveSpeed => MOVE_SPEED;

    public void Enter(PlayerController controller)
    {
        Debug.Log("[MeleeState] ⚔️ Yakın dövüş moduna geçildi.");
        Debug.Log($"[MeleeState] Hız:{MOVE_SPEED} | Hasar:{ATTACK_DAMAGE} | Menzil:{ATTACK_RANGE} | Bekleme:{ATTACK_COOLDOWN}s");
        controller.SetCharacterColor(stateColor);
    }

    public void Update(PlayerController controller)
    {
        // TODO: Melee'ye özel Update mantığı (combo sistemi vb.)
    }

    public void Exit(PlayerController controller)
    {
        Debug.Log("[MeleeState] Exit — Yakın dövüş modundan çıkıldı.");
    }

    public void OnAttack(PlayerController controller)
    {
        if (Time.time - lastAttackTime < ATTACK_COOLDOWN) return;
        lastAttackTime = Time.time;

        Debug.Log("[MeleeState] ⚔️ Yakın dövüş saldırısı!");

        Vector3 origin = controller.AttackPoint != null
            ? controller.AttackPoint.position
            : controller.transform.position + controller.transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin, ATTACK_RANGE, controller.EnemyLayer);

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(ATTACK_DAMAGE);
                Debug.Log($"[MeleeState] {hit.name} → {ATTACK_DAMAGE} hasar!");
            }
        }

        if (hits.Length == 0)
            Debug.Log("[MeleeState] Menzilde düşman yok.");
    }
}
