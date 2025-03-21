using Interfaces;
using penguin;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController _playerControls;

    private void Awake()
    {
        _playerControls = GetComponent<PlayerController>();
    }

    private void Start()
    {
        PlayerControls.BindPlayer(this);
        PlayerControls.EnableGame();
    }
    public void Jump(bool readValueAsButton) => _playerControls.Jump(readValueAsButton);
    public void Attack(bool readValueAsButton) => _playerControls.Attack(readValueAsButton);
    public void Sprint(bool readValueAsButton) => _playerControls.Sprint(readValueAsButton);
    public void Sliding(bool readValueAsButton) => _playerControls.Crouch(readValueAsButton);
    public void Interact(bool readValueAsButton) => _playerControls.Interact(readValueAsButton);
    public void SetMoveDirection(Vector2 moveDirection) => _playerControls.SetMoveDirection(new Vector3(moveDirection.x, 0f, moveDirection.y));
    public void Look(Vector2 lookDirection) => _playerControls.Look(lookDirection);
}
