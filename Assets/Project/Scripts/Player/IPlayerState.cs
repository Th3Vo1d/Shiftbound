using UnityEngine;

public interface IPlayerState
{
    void Enter(PlayerController controller);

    void Update(PlayerController controller);

    void Exit(PlayerController controller);
}
