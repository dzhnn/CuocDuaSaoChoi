
using UnityEngine;

public class Panel : MonoBehaviour
{
    public GameObject panel;
    private void Start()
    {
        panel.SetActive(false);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }
    public void ShowPanel()
    {
        panel.SetActive(true);
    }

}
