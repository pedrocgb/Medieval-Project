using Sirenix.OdinInspector;
using UnityEngine;

public class WorldInventory : MonoBehaviour
{
    private InventoryGridBehaviour _gridBehaviour;
    public InventoryGridBehaviour GridBehaviour => _gridBehaviour;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _chestInventoryPanel;
    private InventoryGridView _chestGridView;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] private string _containerName = "Container";
    public string ContainerName => _containerName;  

    public bool IsOpen { get; private set; }
    public InventoryGrid Grid => _gridBehaviour != null ? _gridBehaviour.Grid : null;

    private void Start()
    {
        if (_chestInventoryPanel != null)
            _chestGridView = _chestInventoryPanel.GetComponent<InventoryGridView>();

        _gridBehaviour = GetComponent<InventoryGridBehaviour>();    
    }

    public void OpenInventory()
    {
        if (IsOpen) return;
        IsOpen = true;

        if (_chestInventoryPanel != null)
            _chestInventoryPanel.SetActive(true);

        if (_chestGridView != null && _gridBehaviour != null)
        {
            //_chestGridView.SetGridSource(_gridBehaviour);
            _chestGridView.Rebuild();
        }
    }

    public void CloseInventory()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (_chestInventoryPanel != null)
            _chestInventoryPanel.SetActive(false);
    }
}
