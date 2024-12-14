
using UnityEngine;

public class CardClass : MonoBehaviour
{
    public GameObject TakeName;
    public Sprite cardImg;
    public PlayerCards player;

    void Start()
    {
        name = TakeName.name;
    }

    void OnMouseDown()
    {
        string originalName = name.Replace("(Clone)", "").Trim();
        Client.Instance.Send("CCardClicked|"+ originalName); 
    }
}
