using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReadyAndSkinPanel : MonoBehaviour
{
    public GameObject readyPanel; 
    public Button readyButton;
    public Image skinImage; 
    public Text readyText;

    private bool isReady = false;

    void Start()
    {
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        readyPanel.SetActive(true); 
        readyText.text = "Chọn skin và nhấn 'Sẵn sàng' để bắt đầu!";
    }

    void OnReadyButtonClicked()
    {
        isReady = !isReady;
        if (isReady)
        {
            readyText.text = "Bạn đã sẵn sàng!";
            Client.Instance.Send("CReady|" + Client.Instance.clientName);
        }
        else
        {
            readyText.text = "Chưa sẵn sàng!";
        }
        CheckAllPlayersReady();
    }

    void CheckAllPlayersReady()
    {
        if (Client.Instance.players.All(player => player.isReady))
        {
            Client.Instance.Send("HStart|");
        }
    }
}
