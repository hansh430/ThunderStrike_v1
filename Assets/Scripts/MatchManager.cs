using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;
    public EventCodes theEvent;
    public List<LeaderboradPlayer> lboardPlayers = new List<LeaderboradPlayer>();

    public enum EventCodes:byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat,
        NextMatch,
        TimeSync
    }
    public enum GameState
    {
        Waiting,
        Playing,
        Ending
      
    }
    public int killToWin = 3;
    public Transform mapCamPint;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding = 5f;
    public bool perpetual;
    public float matchLength=180f;
    public float currentMatchTime;
    public float sendTimer;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetTimer();
            if(!PhotonNetwork.IsMasterClient)
            {
                UIController.instance.timerTxt.gameObject.SetActive(false);
            }
        }
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.Tab) && state !=GameState.Ending)
        {
            if(UIController.instance.leaderBoard.activeInHierarchy)
            {
                UIController.instance.leaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime>0f && state == GameState.Playing)
         {
            currentMatchTime -= Time.deltaTime;
                if (currentMatchTime <= 0)
                {
                    currentMatchTime = 0;
                    state = GameState.Ending;

                    ListPlayersSend();
                    StateCheck();
                }
                UpdateTimerDisplay();
                sendTimer -= Time.deltaTime;
                    if(sendTimer <=0)
                    {
                        sendTimer += 1;
                    TimerSend();
                    }
          }
        }
    }
    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code<200)
        {
            EventCodes theEvents = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
            Debug.Log("Received event: " + theEvents);
            switch(theEvents)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatReceive(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchRecieve();
                    break;
                case EventCodes.TimeSync:
                    TimerRecieve(data);
                    break;
            }
        }
    }
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this); //subscribe the current event
    }
     public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this); //unsubscribe the current event
    }
    public void NewPlayerSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.MasterClient
            },
            new SendOptions
            {
                Reliability = true
            }
            );
    }
    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0],(int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        allPlayers.Add(player);
        ListPlayersSend();
    }
    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count+1];
        package[0] = state;
        for (int i=0; i<allPlayers.Count;i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i+1] = piece;
        }
        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ListPlayers,
           package,
           new RaiseEventOptions
           {
               Receivers = ReceiverGroup.All
           },
           new SendOptions
           {
               Reliability = true
           }
           );
    }
    public void ListPlayersReceive(object[] dataReceived)
    {
        allPlayers.Clear();
        state = (GameState)dataReceived[0];

        for(int i=1; i<dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];
            PlayerInfo player = new PlayerInfo(
                (string)piece[0],(int)piece[1], (int)piece[2], (int)piece[3]);
            allPlayers.Add(player);
            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i-1;
            }
        }
        StateCheck();
    }
    public void UpdateStatSend(int actorSending,int stateToUpdate,int amountToChange)
    {
        object[] package = new object[] { actorSending, stateToUpdate, amountToChange };
         PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            },
            new SendOptions
            {
                Reliability = true
            }
            );
    }
    public void UpdateStatReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];
        for(int i=0; i<allPlayers.Count; i++)
        {
            if(allPlayers[i].actor==actor)
            {
                switch(statType)
                {
                    case 0: //kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " kills " + allPlayers[i].kills);
                        break;
                    case 1: //deaths
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " deaths " + allPlayers[i].deaths);
                        break;
                }
                if(i==index)
                {
                    UpdateStatDisplay();
                }
                if(UIController.instance.leaderBoard.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }
                break;
            }
        }
        ScoreCheck();
    }
    public void UpdateStatDisplay()
    {
        if(allPlayers.Count>index)
        {
            UIController.instance.killTxt.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathTxt.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.instance.killTxt.text = "Kills: 0";
            UIController.instance.deathTxt.text = "Deaths: 0"; 
        }
    }
    void ShowLeaderBoard()
    {
        UIController.instance.leaderBoard.SetActive(true);
        foreach(LeaderboradPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();
        UIController.instance.leaderbordPlayerDisplay.gameObject.SetActive(false);
        List<PlayerInfo> sorted = SortPlayers(allPlayers);
        foreach (PlayerInfo player in sorted)
        {
            LeaderboradPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderbordPlayerDisplay, UIController.instance.leaderbordPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);
            lboardPlayers.Add(newPlayerDisplay);
        }
    }
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();
        while(sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];
            foreach(PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if(player.kills>highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }
        return sorted;
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
    void ScoreCheck()
    {
        bool winnerfound = false;
        foreach(PlayerInfo player in allPlayers)
        {
            if(player.kills>=killToWin && killToWin>0 )
            {
                winnerfound = true;
                break;
            }
        }
        if(winnerfound)
        {
            if(PhotonNetwork.IsMasterClient && state !=GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }
    }
    void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }
    void EndGame()
    {
        state = GameState.Ending;
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        UIController.instance.endScreen.SetActive(true);
        ShowLeaderBoard();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPint.position;
        Camera.main.transform.rotation = mapCamPint.rotation;

        StartCoroutine(EndCo());
    }
    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        if(!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if(PhotonNetwork.IsMasterClient)
            {
                NextMatchSend();
            }
        }
     
    }
    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
          (byte)EventCodes.NextMatch,
          null,
          new RaiseEventOptions
          {
              Receivers = ReceiverGroup.All
          },
          new SendOptions
          {
              Reliability = true
          }
          );
    }
    public void NextMatchRecieve()
    {
        state = GameState.Playing;
        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderBoard.SetActive(false);

        foreach(PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        UpdateStatDisplay();
        PlayerSpawner.instance.SpawnPlayer();
        SetTimer();
    }
    public void SetTimer()
    {
        if(matchLength>0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }
    public void UpdateTimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);
        UIController.instance.timerTxt.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
    }
    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };
        PhotonNetwork.RaiseEvent(
         (byte)EventCodes.TimeSync,
         package,
         new RaiseEventOptions
         {
             Receivers = ReceiverGroup.All
         },
         new SendOptions
         {
             Reliability = true
         }
         );
    }
    public void TimerRecieve(object[] dataRecieved)
    {
        currentMatchTime = (int)dataRecieved[0];
        state = (GameState)dataRecieved[1];
        UpdateTimerDisplay();
        UIController.instance.timerTxt.gameObject.SetActive(true);
    }
}
[System.Serializable]
public class PlayerInfo
{
     public string name;
     public int actor,kills,deaths;
     public PlayerInfo(string _name, int _actor,int _kills, int _deaths)
     {
        name=_name;
        actor=_actor;
        kills = _kills;
        deaths = _deaths;
     }
}
