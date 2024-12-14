using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Net;
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { set; get; }
    public GameObject playerPrefab; //mới
    
    [Header("All Options")]    
    public GameObject ServerMenu;
    public GameObject ConnectMenu;
    public GameObject CreateRoom;
    public GameObject MainMenu;

    [Header("Server - Client")]
    public GameObject serverPrefab;
    public GameObject clientPerfab;

    [Header("Infomation - To Join")]
    public InputField nameInput;

    [Header("Infomation - To Create")]
    public GameObject StartButton;

    void Start()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        ServerMenu.SetActive(false);
        ConnectMenu.SetActive(false);
        CreateRoom.SetActive(false);
        StartButton.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame")
        {
            CreatePlayers();
        }
    }

    private void CreatePlayers()
    {
        List<GameClient> players = Client.Instance.players;

        foreach (var player in players)
        {
            player.PlayerObject = Instantiate(playerPrefab);
            player.PlayerObject.name = "Người chơi: " + player.name; 

        }
        Client.Instance.Send("CClosePanel|");
    }

    public void ConnectButton() // Join Room
    {
        MainMenu.SetActive(false);
        ConnectMenu.SetActive(true);
        Debug.Log("Nhập IP để vào phòng");
    }

    public void HostButton() // Create Room
    {
        string hostIP = GameObject.Find("IPAddress").GetComponent<InputField>().text;
        if(hostIP == "")
        {
            hostIP = "127.0.0.1";
        }

        IPAddress iPAddress = IPAddress.Parse(hostIP);
        
        try
        {
            Server server = Instantiate(serverPrefab).GetComponent<Server>();
            
            server.Init(iPAddress);

            Client client = Instantiate(clientPerfab).GetComponent<Client>();
            client.clientName = nameInput.text;
            client.isHost = true;
            if (client.clientName == "")
            {
                client.clientName = "Chủ Phòng";
            }
            client.ConnectToServer(hostIP, 5000);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        MainMenu.SetActive(false);
        CreateRoom.SetActive(false);
        ServerMenu.SetActive(true);
        
        Debug.Log("Chủ Phòng");
    }

    public void ConnectToServerButton()
    {
        string hostIP = GameObject.Find("HostIP").GetComponent<InputField>().text;
        if(hostIP == "")
        {
            hostIP = "127.0.0.1";
        }

        string portInput = GameObject.Find("Port").GetComponent<InputField>().text;
        int port = int.Parse(portInput);

        try
        {
            Client client = Instantiate(clientPerfab).GetComponent<Client>();
            client.clientName = nameInput.text;
            if (client.clientName == "")
            {
                client.clientName = "Khách";
            }
            client.ConnectToServer(hostIP, 5000);
            ConnectMenu.SetActive(false);

        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void BackButton()
    {
        MainMenu.SetActive(true);
        ConnectMenu.SetActive(false);
        ServerMenu.SetActive(false);
        CreateRoom.SetActive(false);
        Server server = FindObjectOfType<Server>();
        if( server != null ) {
            Destroy(server.gameObject);
         }

        Client client = FindObjectOfType<Client>();
        if (client != null)
        {
            Destroy(client.gameObject);
        }
    }

    public void HidenPanel()
    {
        CreateRoom.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game đã thoát.");
    }

    public void OnClickStartButton()
    {
        Client.Instance.Send("HStart|");
        
    }
}