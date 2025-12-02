using UnityEngine;

public class Debuggers : MonoBehaviour
{
    PlayerBase playerbase;

    private void Awake()
    {
        playerbase = FindObjectOfType<PlayerBase>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            playerbase.UpdateStrengthAttribute(playerbase.PlayerStats);
            playerbase.UpdateDexterityAttribute(playerbase.PlayerStats);
            playerbase.UpdateConstitutionAttribute(playerbase.PlayerStats);
        }
    }
}
