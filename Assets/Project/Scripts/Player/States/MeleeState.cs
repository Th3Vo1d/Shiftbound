using UnityEngine;

public class MeleeState : IPlayerState
{
    // Melee = Kırmızı
    private readonly Color stateColor = new Color(0.93f, 0.25f, 0.20f);

    public void Enter(PlayerController controller)
    {
        Debug.Log("[MeleeState] Enter — Yakın dövüş moduna geçildi.");
        controller.SetCharacterColor(stateColor);
    }

    public void Update(PlayerController controller)
    {
        // TODO: Melee sınıfına özel hareket, saldırı ve input işlemleri
    }

    public void Exit(PlayerController controller)
    {
        Debug.Log("[MeleeState] Exit — Yakın dövüş modundan çıkıldı.");
    }
}
