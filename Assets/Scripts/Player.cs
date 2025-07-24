using Interfaces;
using Inventory;
using penguin;
using UI;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private PlayerController _playerControls;
    [SerializeField] private UIController hudPrefab;


    private void Start()
    {
        _playerControls = GetComponent<PlayerController>();
        _playerControls.BindController(this);
        UIController spanw = Instantiate(hudPrefab, transform);
        spanw.BindToOwner(_playerControls);
        _hotBar = spanw.GetComponentInChildren<HotBar>();
    }

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
