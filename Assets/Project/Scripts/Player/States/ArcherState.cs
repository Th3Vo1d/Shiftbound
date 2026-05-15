using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Okçu — Uzak mesafe state'i.
/// Yüksek hız, orta hasar, mouse yönüne ok fırlatır.
/// State'den çıkıldığında sahnedeki tüm oklar temizlenir.
/// </summary>
public class ArcherState : IPlayerState
{
    private readonly Color stateColor = new Color(0.18f, 0.80f, 0.34f);

    // --- Denge Değerleri ---
    private const float MOVE_SPEED = 7f;
    private const float ATTACK_COOLDOWN = 0.3f;

    private float lastAttackTime = -999f;
    private readonly List<GameObject> activeArrows = new List<GameObject>();

    public float MoveSpeed => MOVE_SPEED;

    public void Enter(PlayerController controller)
    {
        Debug.Log("[ArcherState] 🏹 Okçu moduna geçildi.");
        Debug.Log($"[ArcherState] Hız:{MOVE_SPEED} | Ok Hasarı:15 | Bekleme:{ATTACK_COOLDOWN}s");
        controller.SetCharacterColor(stateColor);
    }

    public void Update(PlayerController controller)
    {
        // TODO: Archer'a özel Update mantığı (nişan alma vb.)
    }

    public void Exit(PlayerController controller)
    {
        // Sahnede kalan tüm okları yok et
        foreach (GameObject arrow in activeArrows)
        {
            if (arrow != null)
                Object.Destroy(arrow);
        }
        activeArrows.Clear();

        Debug.Log("[ArcherState] Exit — Okçu modundan çıkıldı. Oklar temizlendi.");
    }

    public void OnAttack(PlayerController controller)
    {
        if (Time.time - lastAttackTime < ATTACK_COOLDOWN) return;
        lastAttackTime = Time.time;

        if (controller.ArrowPrefab == null)
        {
            Debug.LogWarning("[ArcherState] Arrow prefab atanmamış!");
            return;
        }

        // Zaten yok olmuş okları listeden çıkar
        activeArrows.RemoveAll(arrow => arrow == null);

        Vector3 direction = controller.GetDirectionToMouse();
        Vector3 spawnPos = controller.AttackPoint != null
            ? controller.AttackPoint.position
            : controller.transform.position + direction * 0.5f;

        GameObject newArrow = Object.Instantiate(controller.ArrowPrefab, spawnPos, Quaternion.LookRotation(direction));
        activeArrows.Add(newArrow);

        Debug.Log("[ArcherState] 🏹 Ok fırlatıldı!");
    }
}
