using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboradPlayer : MonoBehaviour
{
    public TMP_Text playerNameTxt, killsTxt, deathTxt;
    public void SetDetails(string playerName, int kills, int deaths)
    {
        playerNameTxt.text = playerName;
        killsTxt.text = kills.ToString();
        deathTxt.text = deaths.ToString();

    }
}
