using UnityEngine;
using UnityEngine.UI;

public class SkinButton : MonoBehaviour
{
    public SkinSelector skinSelector; 
    public int skinIndex; 

    public void OnButtonClick()
    {
        Client.Instance.Send("CSkinChange|" + Client.Instance.clientName + "|" + (skinIndex-1).ToString());
    }
}
