using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class CardMng : MonoBehaviour
{
    [Header("Cards Manager")]
    public List<CardClass> allCards = new List<CardClass>();
    public List<CardClass> cardsInDeck;
    public List<CardClass> usedCards;
    public GameObject busPanel;
    public static CardMng Instance;

    [Header("Card Event")]
    public GameManager gameMng;
    private SpecialTileEventHandler specialTileEventHandler;

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

    public void OnClickDrawCard()
    {
        Client.Instance.Send("CDrawCard|");
    }

    void Start()
    {
        cardsInDeck = new List<CardClass>(allCards);
    }

    public CardClass DrawACard()
    {
        if (cardsInDeck.Count > 0)
        {
            int randomIndex = Random.Range(0, cardsInDeck.Count);
            CardClass drawnCard = cardsInDeck[randomIndex];
            return drawnCard;

        }
        else
        {
            Debug.Log("không còn bài để rút");
            Client.Instance.Send("CDeckEmpty|");
            return null;
        }

    }

    public void AddUsedCardToDeck()
    {
        if (usedCards.Count == 0)
        {
            Debug.Log("Không có bài để nạp vào chồng bài. Người chơi hãy sử dụng bài!!!");
        }
        else
        {
            cardsInDeck.AddRange(usedCards);
            usedCards.Clear();
            Debug.Log("Vừa mới thêm những lá đã dùng vào chồng bài.");
        }
    }

    public void ShowPanel()
    {
        Client.Instance.Send("CShowPanel|");
    }
    public void ClosePanel()
    {
        Client.Instance.Send("CClosePanel|");
    }

    public void ActivateCard(CardClass card, GameObject player)
    {
        string cardName = card.name.Replace("(Clone)", "").Trim();
        Debug.Log("Đã kích hoạt " + cardName);

        switch (cardName)
        {
            case "Giăng dây thừng":
                
                break;

            case "Sinh nhật":
                Client.Instance.Send("CDrawCard|");
                Client.Instance.Send("CDrawCard|");
                break;

            case "A+":
                Client.Instance.Send("CRolled|6");
                break;

            case "Vé xe bus":
                busPanel.gameObject.SetActive(true);
                break;

            case "Lớp trưởng uy quyền":
                break;

            case "Siêu đạo chích":
                break;

            case "Tết thiếu nhi":
                break;

            case "Patin":
                break;

            case "Xí ngầu hoàn hảo":
                break;

            case "Nhả xí vẫy gọi":
                break;

            case "Đôi bạn cùng tiến":
                break;

            case "Mua hàng online":
                Area shop = gameMng.GetRandomArea();

                if (shop != null)
                {
                    Debug.Log("Random ra shop: " + shop.name);
                    if (shop.points > 0)
                    {
                        shop.points--;
                        player.GetComponent<PlayerMovement>().score++;
                    }
                    else Debug.Log(shop.name + "không còn rương để lấy");
                }
                else
                {
                    Debug.Log("k còn shop");
                }
                break;

            case "Tới giờ học":
                break;

            case "Tranh hàng":
                break;

            case "Dừng lại":
                break;

            case "Cúp điện":
                break;

            case "Ăn miếng trả miếng":
                break;

            case "Quá giang":
                break;

            case "Trà lạc quan":
                break;
            case "Sao cũng được":
                break;
            case "Úm ba la xì bùa 1":
                Client.Instance.Send("CRolled|" + "1");
                break;
            case "Úm ba la xì bùa 2":
                Client.Instance.Send("CRolled|" + "2");
                break;
            case "Úm ba la xì bùa 3":
                Client.Instance.Send("CRolled|" + "3");
                break;
            case "Úm ba la xì bùa 4":
                Client.Instance.Send("CRolled|" + "4");
                break;
            case "Úm ba la xì bùa 5":
                Client.Instance.Send("CRolled|" + "5");
                break;
            case "Úm ba la xì bùa 6":
                Client.Instance.Send("CRolled|" + "6");
                break;
            default:
                Debug.Log("Lá này bị huỷ/ đang xem xét/sai tên"); 
                break;
        }
    }
}
