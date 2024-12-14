using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private List<Tile> path = new List<Tile>();

    [Header("Moving")]
    public float moveSpeed = 3f;
    public float pauseDuration = 0.5f;
    public Tile currentTile;
    public bool isAtSchool = false;
    public bool IsMoving = false;

    [Header("Score")]
    public TextMeshProUGUI point;
    public int score;
    private SpecialTileEventHandler specialTileEventHandler = new SpecialTileEventHandler();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetPath(List<Tile> newPath)
    {
        if (IsMoving) return;
        path = newPath;
        StartCoroutine(MoveAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        IsMoving = true;

        foreach (Tile tile in path)
        {
            Vector3 targetPosition = tile.transform.position + new Vector3(0f, 1f / 5, 0f);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            GameManager.Instance.currentTile = tile;
            yield return new WaitForSeconds(pauseDuration);
        }


        IsMoving = false;

        Tile finalTile = GameManager.Instance.currentTile;
        currentTile = finalTile;
        if (finalTile.GetSpecialTileType() == SpecialTileType.ShopTiles)
        {
            Debug.Log("Shop tile reached, adding points.");
            Client.Instance.Send("CAddPoint|" + finalTile.area.name);
        }
        else if (finalTile.IsSpecialTile())
        {
            Client.Instance.Send("CSpecialTile|" + finalTile.name);
        }
        else
        {
            Debug.Log("kp o dac biet");
        }

    }

    private void Start()
    {
        score = 0;
    }

}
