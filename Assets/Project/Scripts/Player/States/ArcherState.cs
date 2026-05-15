using UnityEngine;

public class ArcherState : IPlayerState
{
    // Archer = Yeşil
    private readonly Color stateColor = new Color(0.18f, 0.80f, 0.34f);

    public void Enter(PlayerController controller)
    {
        Debug.Log("[ArcherState] Enter — Okçu moduna geçildi.");
        controller.SetCharacterColor(stateColor);
    }

    public void Update(PlayerController controller)
    {
        // TODO: Archer sınıfına özel hareket, saldırı ve input işlemleri
    }

    public void Exit(PlayerController controller)
    {
        Debug.Log("[ArcherState] Exit — Okçu modundan çıkıldı.");
    }
}
