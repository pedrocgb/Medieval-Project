using UnityEngine;

public class WorldInteractable : MonoBehaviour
{
    private WorldInventory worldInventory;

    private void Start()
    {
        worldInventory = GetComponent<WorldInventory>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                worldInventory?.OpenInventory();
            }
        }
    }
}
