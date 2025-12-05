using UnityEngine;
using CharactersStats;

public class Debuggers : MonoBehaviour
{
    PlayerBase playerbase;

    private void Awake()
    {
        playerbase = FindFirstObjectByType<PlayerBase>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Player Head protection: " + playerbase.GetStat(StatId.HeadProtection).Value);
            Debug.Log("Player Cold res: " + playerbase.GetStat(StatId.ColdResistance).Value);
        }
    }
}
