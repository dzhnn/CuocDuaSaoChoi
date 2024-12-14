using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Manager")]
    public Pathfinding pathfinding; 
    public float moveSpeed = 3f; 
    public TextMeshProUGUI diceRes;
    public Tile currentTile;
    private List<Tile> possibleDestinations = new List<Tile>();

    [Header("Pointer")]
    public GameObject pointerPrefab;  
    private List<GameObject> activePointers = new List<GameObject>();
    private SpecialTileEventHandler specialTileEventHandler;

    private Client client;

    [Header("Ready")]
    public GameObject ReadyPanel;

    [Header("Chat")]
    public Transform chatMessageContainer;
    public GameObject messagePrefab;

    [Header("Dice & Moving")]
    public TextMeshProUGUI DiceRes, PlayerScore;
    public List<Tile> allTiles;
    private int diceValue = 0;

    [Header("Area")]
    public List<Area> allArea;

    [Header("PlayerList")]
    public GameObject playerinfoPrefab;
    public GameObject playersPanel;

    private void Start()
    {
        currentTile = allTiles[0];
        client = FindObjectOfType<Client>();

        Client.Instance.StartTurn(0);
        

    }

    public void CreatePlayerList()
    {
        foreach (Transform child in playersPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<GameClient> pls = Client.Instance.players;
        foreach (GameClient gc in pls)
        {
            GameObject po = gc.PlayerObject;

            GameObject newRank = Instantiate(playerinfoPrefab);
            newRank.transform.SetParent(playersPanel.transform, false);
            RectTransform rectTransform = newRank.GetComponent<RectTransform>();
            newRank.transform.localScale = new Vector3(0.2f, 0.2f, 1f);


            Image ava = newRank.transform.Find("Image").GetComponent<Image>();
            Text name = newRank.transform.Find("Name").GetComponent<Text>();

            ava.sprite = gc.PlayerObject.GetComponent<SpriteRenderer>().sprite;
            name.text = gc.name;
        }
    }
    
    public void UpdateDiceRes(int p)
    {
        DiceRes.text = p.ToString();
    }
    
    public void UpdatePlayerScore(int p)
    {
        PlayerScore.text = p.ToString();
    }

    public Tile GetCurrentTile(GameObject player)
    {
        float minDistance = Mathf.Infinity;
        Tile nearestTile = null;

        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                float distance = Vector3.Distance(player.transform.position, tile.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTile = tile;
                }
            }
        }

        return nearestTile;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void OnClickRollDice()
    {
        Client.Instance.Send("CROLL|" + Client.Instance.clientName);
    }
    
    public int RandomDice()
    {
        return Random.Range(1, 7);
    }
    
    public void MoveWithDiceNumber(Tile curTile, int diceNum, GameObject player)
    {
        diceValue = diceNum;
        currentTile = curTile;
        currentTile = GetCurrentTile(player);

        ClearActivePointers();
        possibleDestinations = pathfinding.GetPossibleDestinations(currentTile, diceValue);

        foreach (Tile destination in possibleDestinations)
        {
            GameObject newPointer = Instantiate(pointerPrefab, destination.transform.position, Quaternion.identity);
            newPointer.GetComponent<PointerBounce>().StartBouncing();
            activePointers.Add(newPointer);
        }
    }
    
    private void ClearActivePointers()
    {
        foreach (GameObject pointer in activePointers)
        {
            Destroy(pointer);
        }
        activePointers.Clear();
    }
    
    public void SelectDestinationTile(Tile destinationTile, GameObject player)
    {
        if (possibleDestinations.Contains(destinationTile))
        {
            MoveToTile(destinationTile, player);
            ClearActivePointers();
            ResetDestinationColors();
        }
        else
        {
            Debug.Log("Selected tile is not a valid destination.");
        }
    }
    
    public void MoveToTile(Tile destinationTile, GameObject player)
    {
        currentTile = GetCurrentTile(player);
        if (destinationTile != currentTile)
        {
            List<Tile> path = pathfinding.GetPath(currentTile, destinationTile, diceValue);

            if (path.Count > 0)
            {
                player.GetComponent<PlayerMovement>().SetPath(path);
            }
            else
            {
                Debug.Log("No path found to the destination.");
            }
        }
    }
    
    private void ResetDestinationColors()
    {
        foreach (Tile tile in possibleDestinations)
        {
            tile.GetComponent<Renderer>().material.color = Color.white;
        }
        possibleDestinations.Clear();
    }
    
    public Area GetRandomArea()
    {
        int randomIndex = Random.Range(0, allArea.Count);
        return allArea[randomIndex];
    }

    //QUẢN LÝ LƯỢT CHƠI
    public void OnEndTurnButtonClicked()
    {
        Client.Instance.Send("CENDTURN|");
    }

    //CHAT
    public void ChatMessage(string msg)
    {
        if (messagePrefab == null)
        {
            Debug.Log("messagePrefab is not assigned.");
            return;
        }
        if (chatMessageContainer == null)
        {
            Debug.Log("chatMessageContainer is not assigned.");
            return;
        }

        GameObject go = Instantiate(messagePrefab) as GameObject;
        go.transform.SetParent(chatMessageContainer);
        Text textComponent = go.GetComponentInChildren<Text>();
        
        if (textComponent == null)
        {
            Debug.Log("No Text component found in messagePrefab.");
            return;
        }

        textComponent.text = msg;
        go.transform.localScale = Vector3.one;

    }

    public void SendChatMessage()
    {
        InputField i = GameObject.Find("MessageInput").GetComponent<InputField>();
        if (i.text == "") return;

        Client.Instance.Send("CMSG|" + i.text);

        i.text = "";
    }

    public void ClickHiden()
    {
        ReadyPanel.SetActive(false);
    }
}
