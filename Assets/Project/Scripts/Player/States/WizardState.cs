using UnityEngine;

public class WizardState : IPlayerState
{
    // Wizard = Mor
    private readonly Color stateColor = new Color(0.55f, 0.24f, 0.88f);

    public void Enter(PlayerController controller)
    {
        Debug.Log("[WizardState] Enter — Büyücü moduna geçildi.");
        controller.SetCharacterColor(stateColor);
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
