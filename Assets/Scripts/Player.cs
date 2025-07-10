using Interfaces;
using Inventory;
using penguin;
using UI;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private PlayerController _playerControls;
    private HotBar _hotBar;
    [SerializeField] private UIController hudPrefab;


    private void Start()
    {
        _playerControls = GetComponent<PlayerController>();
        if (true != true) return;
        
        _playerControls.BindController(this);
        UIController spanw = Instantiate(hudPrefab, transform);
        spanw.BindToPenguin(_playerControls);
        _hotBar = spanw.GetComponentInChildren<HotBar>();
        Debug.Log(spanw.name);
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
    public void SetSelected(int key) => _hotBar.UpdateScrollIndex(key);
    public void ScrollSelected(float scroll) => _hotBar.UpdateScrollSlot((int)scroll);
    public bool HeyIPickedSomethingUp(ItemStats iItemStats)
    { 
        return _hotBar.HeyIPickedSomethingUp(iItemStats);
    }
    
    [ClientRpc]
    public void EquipItem_ClientRpc()
    {
            
    }
        
    [ClientRpc]
    public void UnequipItem_ClientRpc()
    {
            
    }
        
    [ServerRpc(RequireOwnership = true)]
    public void EquipItem_ServerRpc()
    {
        EquipItem_ClientRpc();
    }
        
    [ServerRpc(RequireOwnership = true)]
    public void UnequipItem_ServerRpc()
    {
        UnequipItem_ClientRpc();
    }
}
