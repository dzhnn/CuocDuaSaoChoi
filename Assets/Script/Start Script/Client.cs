using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public string clientName;
    public bool isHost = false;

    [Header("Socket")]
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    [Header("Players")]
    public int currentPlayerIndex = 0;
    public List<GameClient> players = new List<GameClient>();

    [Header("End")]
    public GameObject AvaNamePrefab;
    private GameObject RankPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Kết nối tới Server
    public bool ConnectToServer(string host, int port)
    {
        if (socketReady) return false;
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }
        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnInComingData(data);
                }
            }

        }
    }

    public void Send(string data)
    {
        if (!socketReady) return;
        writer.WriteLine(data);
        writer.Flush();
    }

    // Các thông điệp
    private void OnInComingData(string data)
    {
        Debug.Log("Server broadcasted: " + data);
        string[] aData = data.Split('|');

        GameManager gameM = GameManager.Instance;
        GameClient gc;
        PlayerCards p;
        PlayerMovement pm;
        SpecialTileEventHandler steh;
        steh = gameObject.AddComponent<SpecialTileEventHandler>();


        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                Send("CWHO|" + clientName);
                break;

            case "SCNN":
                UserConnected(aData[1], false);
                break;

            case "CStart":
                SceneManager.LoadScene("MainGame");
                break;

            case "SENDTURN":
                EndTurn(); break;


            //XX VÀ DI CHUYỂN
            case "SRandomDice": //chỉ gửi cho 1 người
                int diceValue = gameM.RandomDice(); // Lấy số xúc xắc
                Debug.Log($"Dice rolled: {diceValue} for player {players[currentPlayerIndex].name}");
                players[currentPlayerIndex].rolled = true;
                //thêm hàm cập nhật số xúc xắc
                Send("CRolled|" + diceValue.ToString());
                break;

            case "SFindDes": //gửi cho tất cả
                Tile currentT = gameM.GetCurrentTile(players[currentPlayerIndex].PlayerObject);
                int dice = int.Parse(aData[1]);
                gameM.UpdateDiceRes(dice);
                gameM.MoveWithDiceNumber(currentT, dice, players[currentPlayerIndex].PlayerObject);
                break;

            case "SDice":
                int d = int.Parse(aData[1]);
                gameM.UpdateDiceRes(d);
                break;

            case "SStartMove": //gửi cho tất cả
                Tile desTile = GameObject.Find(aData[1]).GetComponent<Tile>();
                GameManager.Instance.SelectDestinationTile(desTile, players[currentPlayerIndex].PlayerObject);
                break;

            case "SNotiScore": //1 là điểm, 2 là tên gc, 3 là tên area
                Area a = gameM.allArea.Find(a => a.name == aData[3]);
                if (a != null && a.points > 0)
                {
                    a.points--;
                }
                gc = GetGameClientByName(aData[2]);
                gc.PlayerObject.GetComponent<PlayerMovement>().score = int.Parse(aData[1]);
                break;

            case "SUpdateScore":
                gc = GetGameClientByName(clientName);
                pm = gc.PlayerObject.GetComponent<PlayerMovement>();
                if (aData[1] == "--")
                {
                    pm.score--;
                    Send("CNotiScore|" + pm.score.ToString() + "|" + clientName);

                }
                else if (aData[1] == "++")
                {
                    Area ar = gameM.allArea.Find(a => a.name == aData[2]);
                    if (ar != null && ar.points > 0)
                    {
                        pm.score++;
                        Send("CNotiScore|" + pm.score.ToString() + "|" + clientName + "|" + ar.name);
                    }
                }
                gameM.UpdatePlayerScore(pm.score);

                break;



            //SPECIAL TILE
            case "SSpecialTile":
                Tile special = GameObject.Find(aData[1]).GetComponent<Tile>();
                gc = GetGameClientByName(aData[2]);
                steh.HandleSpecialTileEvent(special, gc.PlayerObject);
                break;

            case "SToSchool":
                Debug.Log(aData[2] + " is now at school.");
                gc = GetGameClientByName(aData[2]);
                if (!gc.myTurn) break;
                gc.atSchool = true;
                Tile t = gameM.allTiles.Find(tl => tl.name == aData[1]);
                Tile targetTile = t.GetTargetTile();
                pm = gc.PlayerObject.GetComponent<PlayerMovement>();
                pm.StartCoroutine(steh.SMoveToTile(targetTile, pm));
                break;

            case "SGraduate":
                Debug.Log(aData[2] + " is graduate!!");
                gc = GetGameClientByName(aData[2]); gc.atSchool = false;
                gc.rolled = false;
                Tile target = GameObject.Find(aData[1]).GetComponent<Tile>();
                gc.PlayerObject.GetComponent<PlayerMovement>().StartCoroutine(steh.SMoveToTile(target, gc.PlayerObject.GetComponent<PlayerMovement>()));
                break;

            case "SMoveBus":
                gc = GetGameClientByName(aData[2]);
                Tile bus = gameM.allTiles.Find(tl => tl.name == aData[1]);
                pm = gc.PlayerObject.GetComponent<PlayerMovement>();
                pm.StartCoroutine(steh.SMoveToTile(bus, pm));
                break;

            case "SMeteor":
                gc = GetGameClientByName(clientName);
                Area area = gameM.allArea.Find(a => a.name == aData[1]);
                area.points = 0;
                area.CreateOverlay();
                Debug.Log("Randomly selected area: " + area.name);
                steh.CheckAndMinusPoints(area, gc.PlayerObject);

                gameM.allArea.Remove(area);
                if (gameM.allArea.Count == 0) Send("CENDGAME|");
                break;


            //CARD
            case "SDraw1Card":
                gc = GetGameClientByName(aData[1]);
                gc.PlayerObject.GetComponent<PlayerCards>().DrawCardForPlayer();
                break;

            case "SDeck--":
                CardClass toremove = CardMng.Instance.cardsInDeck.Find(card => card.name == aData[1]);
                CardMng.Instance.cardsInDeck.Remove(toremove);
                break;

            case "SDeckEmpty":
                CardMng.Instance.AddUsedCardToDeck();
                break;

            case "SShowPanel":
                gc = GetGameClientByName(clientName); //ban than
                gc.PlayerObject.GetComponent<PlayerCards>().ShowCardPanel();
                break;

            case "SClosePanel":
                gc = GetGameClientByName(clientName); //ban than
                gc.PlayerObject.GetComponent<PlayerCards>().CloseCardPanel();
                break;

            case "SShowCardOption":
                gc = GetGameClientByName(aData[2]);
                p = gc.PlayerObject.GetComponent<PlayerCards>();
                CardClass selected = p.myCards.Find(card => card.name == aData[1]);

                if (p.deletingExtraCard)
                {
                    p.OnClickDeleteCard(selected);
                }
                else
                {
                    p.selectedCard = selected;
                    p.ShowCardOptions(selected);
                }

                break;

            case "SUse": // 
                gc = GetGameClientByName(clientName); //ban than
                CardClass c = CardMng.Instance.allCards.Find(card => card.name == aData[1]);
                CardMng.Instance.ActivateCard(c, gc.PlayerObject);
                break;

            case "SAddToUsed": // ten card k co Clone
                gc = GetGameClientByName(clientName); //tat ca mng
                p = gc.PlayerObject.GetComponent<PlayerCards>();
                CardClass card = CardMng.Instance.allCards.Find(card => card.name == aData[1]);

                CardMng.Instance.usedCards.Add(card);

                break;

            case "SMSG":
                GameManager.Instance.ChatMessage(aData[1]);
                break;

            case "SSkinChange":
                string playerName = aData[1];
                int skinIndex = int.Parse(aData[2]);
                GameClient client = GetGameClientByName(playerName);
                if (client != null)
                {
                    client.PlayerObject.GetComponent<SkinSelector>().ChangeSkin(skinIndex);
                }

                gameM.CreatePlayerList();
                break;

            case "SSort":
                SortPlayersByScore();
                SceneManager.LoadScene("EndScene");
                Send("CLoaded|");
                break;

            case "SRank":
                RankPanel = GameObject.Find("RankPanel");
                CreateRank();
                break;
        }
    }

    private void UserConnected(string name, bool host)
    {
        GameClient client = new GameClient();
        client.name = name;

        players.Add(client);

        if (players.Count >= 1)
        {
            Debug.Log("Có thể bắt đầu Game");
            MenuManager.Instance.StartButton.SetActive(true);
        }
    }

    // Đóng socket
    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
    private void CloseSocket()
    {
        if (!socketReady) return;
        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

    //End game
    void CreateRank()
    {
        foreach (Transform child in RankPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (GameClient gc in players)
        {
            GameObject po = gc.PlayerObject;
            GameObject AvaNamePrefab = gc.PlayerObject.GetComponent<SkinSelector>().AvaNamePrefab;

            GameObject newRank = Instantiate(AvaNamePrefab);
            newRank.transform.SetParent(RankPanel.transform, false);
            RectTransform rectTransform = newRank.GetComponent<RectTransform>();
            newRank.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

          
            Image ava = newRank.transform.Find("Image").GetComponent<Image>();
            Text name = newRank.transform.Find("Name").GetComponent<Text>();
            Text score = newRank.transform.Find("Score").GetComponent<Text>();

            ava.sprite = gc.PlayerObject.GetComponent<SpriteRenderer>().sprite;
            name.text = gc.name;
            score.text = po.GetComponent<PlayerMovement>().score.ToString();

            po.gameObject.SetActive(false);
        }

    }

    public void SortPlayersByScore()
    {
        players = players.OrderByDescending(player =>
        {
            var playerScoreComponent = player.PlayerObject.GetComponent<PlayerMovement>();
            return playerScoreComponent != null ? playerScoreComponent.score : 0;
        }).ToList();
    }

    public GameClient GetGameClientByName(string playerName)
    {
        return players.Find(p => p.name == playerName);
    }

    // Theo lượt
    public void EndTurn()
    {
        players[currentPlayerIndex].rolls = 0;
        players[currentPlayerIndex].myTurn = false;
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            Send("CMeteor|"); //nổ xốp
            currentPlayerIndex = 0; // Quay về người chơi đầu tiên nếu đã hết lượt
        }
        StartTurn(currentPlayerIndex);
    }
    public void StartTurn(int index)
    {
        GameClient p = GetGameClientByName(players[index].name);
        p.banned--;
        Debug.Log(players[index].banned);
        p.rolls = 0;
        p.myTurn = true;
        p.rolled = false;
        Debug.Log($"Current index is " + index + " It's " + p.name + "'s turn.");
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
    public bool myTurn;
    public bool rolled;
    public bool isReady;
    public bool atSchool;
    public int banned;
    public int rolls = 0;
    public GameObject PlayerObject;
}