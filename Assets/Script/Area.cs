using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public int points; 
    public List<Tile> trigger;

    public Sprite overlaySprite; 
    private GameObject overlayObject;

    public void CreateOverlay()
    {
        if (overlaySprite == null)
        {
            Debug.LogWarning("Overlay sprite chưa được gán cho " + gameObject.name);
            return;
        }

        overlayObject = new GameObject("OverlayImage");

        overlayObject.transform.SetParent(this.transform);
        overlayObject.transform.localPosition = Vector3.zero;
        overlayObject.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
        SpriteRenderer overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
        overlayRenderer.sprite = overlaySprite;

        overlayRenderer.sortingLayerName = "Player";
        overlayRenderer.sortingOrder = 1;
    }
}

