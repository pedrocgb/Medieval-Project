using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Player _controlScheme;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _inventoryPanel;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _equipmentPanel;

    private void Awake()
    {
        _controlScheme = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (_controlScheme.GetButtonDown("Toggle Inventory"))
        {
            ToggleInventory();
            ToggleEquipment();
        }
    }

    private void ToggleInventory()
    {
        bool enable = !_inventoryPanel.activeSelf;
        _inventoryPanel.SetActive(enable);
    }

    private void ToggleEquipment()
    {
        bool enable = !_equipmentPanel.activeSelf;
        _equipmentPanel.SetActive(enable);
    }
}
