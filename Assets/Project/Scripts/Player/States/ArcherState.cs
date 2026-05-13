using UnityEngine;

public class ArcherState : IPlayerState
{
    public void Enter(PlayerController controller)
    {
        Debug.Log("[ArcherState] Enter — Okçu moduna geçildi.");
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
