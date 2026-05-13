using UnityEngine;

public class WizardState : IPlayerState
{
    public void Enter(PlayerController controller)
    {
        Debug.Log("[WizardState] Enter — Büyücü moduna geçildi.");
    }

    public void Update(PlayerController controller)
    {
        // TODO: Wizard sınıfına özel hareket, saldırı ve input işlemleri
    }

    public void Exit(PlayerController controller)
    {
        Debug.Log("[WizardState] Exit — Büyücü modundan çıkıldı.");
    }
}
