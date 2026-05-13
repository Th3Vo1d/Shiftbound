using UnityEngine;

public class MeleeState : IPlayerState
{
    public void Enter(PlayerController controller)
    {
        Debug.Log("[MeleeState] Enter — Yakın dövüş moduna geçildi.");
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
