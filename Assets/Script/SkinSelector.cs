using UnityEngine;
using System.Collections.Generic;

public class SkinSelector : MonoBehaviour
{
    public GameObject characterPrefab; 
    private SpriteRenderer spriteRenderer; 

    public GameObject AvaNamePrefab; 
    public List<Sprite> skins = new List<Sprite>();

    void Start()
    {
        spriteRenderer = characterPrefab.GetComponent<SpriteRenderer>();
        if (skins.Count == 0)
        {
            Debug.LogError("Cần phải có ít nhất một skin.");
        }
    }

    // Hàm thay đổi skin theo index
    public void ChangeSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex <= skins.Count)
        {
            spriteRenderer.sprite = skins[skinIndex];
        }
        else
        {
            Debug.LogWarning("Skin index không hợp lệ.");
        }
    }

}
