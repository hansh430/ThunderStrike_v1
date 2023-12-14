using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public TMP_Text overHeatedMsg;
    public static UIController instance;
    public Slider weaponTempSlider;
    public GameObject dieScreen;
    public TMP_Text dieMessage;
    public Slider healthSlider;
    public TMP_Text killTxt;
    public TMP_Text deathTxt;
    public GameObject leaderBoard;
    public LeaderboradPlayer leaderbordPlayerDisplay;
    public GameObject endScreen;
    public TMP_Text timerTxt;
    public GameObject optionScreen;
    public bool isPaused;
    public Button shootBtn;
    public Button jumpBtn;
    public Button cameraZoomButton;
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOption();
        }
        if (optionScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void ShowHideOption()
    {
        if(!optionScreen.activeInHierarchy)
        {
            optionScreen.SetActive(true);
            isPaused = true;
        }
        else
        {
            optionScreen.SetActive(false);
            isPaused = false;
        }
    }
   public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
