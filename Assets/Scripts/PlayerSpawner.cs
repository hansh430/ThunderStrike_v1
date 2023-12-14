using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    public GameObject playerPrefab;
    private GameObject player;
    public GameObject deathEffect;
    public FixedJoystick joystick;
    public Button shootBtn;
    public Button jumpBtn;

    public float respawnTime = 5f;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }
    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        PlayerController pc = player.GetComponent<PlayerController>();
        SetPlayerJoystick(pc);
    }
    public void SetPlayerJoystick(PlayerController player)
    {
        player.joyStick = joystick;
    }
    public void Die(string damager)
    {
        UIController.instance.dieMessage.text = "You were killed by " + damager;
        MatchManager.instance.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
        if (player != null)
        {
            StartCoroutine(DieCo(respawnTime));
        }
    }
    public IEnumerator DieCo(float duration)
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;
        UIController.instance.dieScreen.SetActive(true);
        yield return new WaitForSeconds(duration);
        UIController.instance.dieScreen.SetActive(false);
        if (MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }
    }
}
