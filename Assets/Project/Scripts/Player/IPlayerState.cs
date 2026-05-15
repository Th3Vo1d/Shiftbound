using UnityEngine;

public interface IPlayerState
{
    float MoveSpeed { get; }
    void Enter(PlayerController controller);
    void Update(PlayerController controller);
    void Exit(PlayerController controller);
    void OnAttack(PlayerController controller);
}
