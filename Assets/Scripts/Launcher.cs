using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] TMP_Text loadingTxt;
    [SerializeField] GameObject menuBtn;
    [SerializeField] GameObject createRoomScreen;
    [SerializeField] TMP_InputField roomName;
    [SerializeField] TMP_Text roomNameTxt,playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    [SerializeField] GameObject roomScreen;
    [SerializeField] TMP_Text errorTxt;
    [SerializeField] GameObject errorScreen;
    [SerializeField] GameObject roomBrowserScreen;
    [SerializeField] RoomButtonScript roomBtn;
    private List<RoomButtonScript> allRoomBtns= new List<RoomButtonScript>();
    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    public static bool hasSetNickName;
    public string levelToPlay;
    public GameObject startGameBtn;
    public GameObject testRoomBtn;
    public string[] allMaps;
    public bool changeMapBetweenRounds = true;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        CloseMenu();
        loadingScreen.SetActive(true);
        loadingTxt.text = "Connecting to the network.....";
        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
#if UNITY_EDITOR
        testRoomBtn.SetActive(true);
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingTxt.text = "Joining Lobby.....";

    }
    public override void OnJoinedLobby()
    {
        CloseMenu();
        menuBtn.SetActive(true);
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!hasSetNickName)
        {
            CloseMenu();
            nameInputScreen.SetActive(true);
            if(PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }
    public void CloseMenu()
    {
        loadingScreen.SetActive(false);
        menuBtn.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }
    public void OpenCreateRoom()
    {
        CloseMenu();
        createRoomScreen.SetActive(true);
    }
    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(roomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomName.text, options);
            CloseMenu();
            loadingTxt.text = "Creating Room.....";
            loadingScreen.SetActive(true);
        }
    }
    public override void OnJoinedRoom()
    {
        CloseMenu();
        roomScreen.SetActive(true);
        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if(PhotonNetwork.IsMasterClient)
        {
            startGameBtn.SetActive(true);
        }
        else
        {
            startGameBtn.SetActive(false);
        }
    }
    public void ListAllPlayers()
    {
        foreach(TMP_Text item in allPlayerNames)
        {
            Destroy(item.gameObject);
        }
        allPlayerNames.Clear();
        Player[] player = PhotonNetwork.PlayerList;
        for(int i=0; i<player.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = player[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayerNames.Add(newPlayerLabel);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayerNames.Add(newPlayerLabel);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorTxt.text = "Failed to create Room: " + message;
        CloseMenu();
        errorScreen.SetActive(true);
    }
    public void CloseErrorScreen()
    {
        CloseMenu();
        menuBtn.SetActive(true);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenu();
        loadingTxt.text = "Leaving Room.....";
        loadingScreen.SetActive(true);
    }
    public override void OnLeftRoom()
    {
        CloseMenu();
        menuBtn.SetActive(true);
    }
    public void OpenRoomBrowser()
    {
        CloseMenu();
        roomBrowserScreen.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        CloseMenu();
        menuBtn.SetActive(true);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("run:"+roomList.Count);
        foreach (RoomButtonScript item in allRoomBtns)
        {
            Destroy(item.gameObject);
        }
        allRoomBtns.Clear();
        roomBtn.gameObject.SetActive(false);
        for (int i=0; i<roomList.Count;i++)
        {
            if(roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButtonScript newButton = Instantiate(roomBtn, roomBtn.transform.parent);
                newButton.SetButtonDetail(roomList[i]);
                newButton.gameObject.SetActive(true);
                allRoomBtns.Add(newButton);
                Debug.Log("run2:" + roomList.Count);
            }
        }
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenu();
        loadingTxt.text = "Joining Room.....";
        loadingScreen.SetActive(true);
    }
    public void SetNickName()
    {
        if(!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString("playerName", nameInput.text);
            CloseMenu();
            menuBtn.SetActive(true);
            hasSetNickName = true;
        }
    }
    public void StartGame()
    {
        // PhotonNetwork.LoadLevel(levelToPlay);
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameBtn.SetActive(true);
        }
        else
        {
            startGameBtn.SetActive(false);
        }
    }
    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom("Test",options);
        CloseMenu();
        loadingTxt.text = "Creating Room.....";
        loadingScreen.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
} 
