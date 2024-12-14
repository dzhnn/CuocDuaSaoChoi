
using System.Collections.Generic;
using UnityEngine;



public class PlayerCards : MonoBehaviour
{

    [Header("Card")]
    public List<CardClass> myCards;
    public Canvas DetailCanvas;
    public UnityEngine.UI.Image cardImageDisplay;

    [Header("Panel & Button")]
    public GameObject cardPanel;
    public UnityEngine.UI.Button showpanel, closepanel, DelButton, UseButton;

    [Header("For Card Func")]
    public GameObject ChooseCardPanel;
    public bool deletingExtraCard;
    public CardClass selectedCard, originalCard;
    
    void Start()
    {
        deletingExtraCard = false;

        cardImageDisplay = GameObject.Find("Image").GetComponent<UnityEngine.UI.Image>();

        cardPanel = GameObject.Find("CardPanel");
        showpanel = GameObject.Find("ShowPanel").GetComponent<UnityEngine.UI.Button>();
        closepanel = GameObject.Find("ClosePanel").GetComponent<UnityEngine.UI.Button>();
        ChooseCardPanel = GameObject.Find("ChooseCardPanel");

        DelButton = GameObject.Find("DeleteCardButt").GetComponent<UnityEngine.UI.Button>();
        UseButton = GameObject.Find("UseCardButt").GetComponent<UnityEngine.UI.Button>();
        DelButton.onClick.AddListener(OnClickDeleteCardNoParam);
        UseButton.onClick.AddListener(OnClickUseCard);

        DetailCanvas = GameObject.Find("CardOptionsCanvas").GetComponent<Canvas>();
    }

    public void DrawCardForPlayer()
    {
        if (deletingExtraCard)
        {
            Debug.Log("Đang xoá bài. Không được rút thêm");
            return;
        }

        CardClass newCard = CardMng.Instance.DrawACard();
        if (newCard != null)
        {
            if (myCards.Count < 4)
            {
                myCards.Add(newCard);
                Debug.Log("Player rút được: " + newCard.name);
                ShowCardPanel();
            }
            else
            {
                Debug.Log("Xoá bớt 1 lá bài trên tay rồi rút lại.");
                ChooseCardToDelete();
                myCards.Add(newCard);
                Debug.Log("Sau khi xoá. Player rút được: " + newCard.name);
            }

            Client.Instance.Send("CCardDrawed|" + newCard.name);
        }
        else
        {
            Debug.Log("newCard bị null.");
        }
    }

    public void ChooseCardToDelete()
    {
        CloseCardPanel();
        ChooseCardPanel.gameObject.SetActive(true);
        deletingExtraCard = true;

        if (myCards.Count == 0) { return; }

        foreach (Transform child in ChooseCardPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (CardClass card in myCards)
        {
            GameObject cardToClone = CardMng.Instance.allCards.Find(c => c.GetComponent<CardClass>().name == card.name).gameObject;

            if (cardToClone != null)
            {
                GameObject newCardObject = Instantiate(cardToClone);
                newCardObject.transform.SetParent(ChooseCardPanel.transform, false);
                RectTransform rectTransform = newCardObject.GetComponent<RectTransform>();
                newCardObject.transform.localScale = new Vector3(80f, 80f, 1f);
            }
        }
    }

    public void ShowCardPanel()
    {
        cardPanel.SetActive(true);
        DisplayAllMyCards();
        showpanel.gameObject.SetActive(false);
        closepanel.gameObject.SetActive(true);
        ChooseCardPanel.gameObject.SetActive(false);
        DetailCanvas.gameObject.SetActive(false);
    }
    public void CloseCardPanel()
    {
        cardPanel.SetActive(false);
        closepanel.gameObject.SetActive(false);
        showpanel.gameObject.SetActive(true);
        DetailCanvas.gameObject.SetActive(false);
        ChooseCardPanel.gameObject.SetActive(false);
    }
    
    public void DisplayAllMyCards()
    {
        foreach (Transform child in cardPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (CardClass card in myCards)
        {
            GameObject cardToClone = CardMng.Instance.allCards.Find(c => c.GetComponent<CardClass>().name == card.name).gameObject;

            if (cardToClone != null)
            {
                GameObject newCardObject = Instantiate(cardToClone);
                newCardObject.transform.SetParent(cardPanel.transform, false);
                RectTransform rectTransform = newCardObject.GetComponent<RectTransform>();
                newCardObject.transform.localScale = new Vector3(30f, 30f, 1f);
            }
        }
    }

    public void ShowCardOptions(CardClass card)
    {
        DetailCanvas.gameObject.SetActive(true);
        selectedCard = card;
        if (cardImageDisplay != null)
        {
            cardImageDisplay.sprite = card.cardImg;
        }
    }

    public void OnClickDeleteCard(CardClass card)
    {
        myCards.Remove(card);
        ShowCardPanel(); 
        DetailCanvas.gameObject.SetActive(false);
        Client.Instance.Send("CAddToUsed|" + card.name);
        deletingExtraCard = false;
    }
    
    public void OnClickUseCard()
    {
        Client.Instance.Send("CUse|" + selectedCard.name);
        OnClickDeleteCard(selectedCard);
        DetailCanvas.gameObject.SetActive(false);
    }

    public void OnClickDeleteCardNoParam()
    {
        OnClickDeleteCard(selectedCard); 
    }

}