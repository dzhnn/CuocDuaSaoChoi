using System.Collections;
using UnityEngine;

public class SpecialTileEventHandler : MonoBehaviour
{
    public Tile destinationTile;
    public Panel panel;

    public void HandleSpecialTileEvent(Tile specialTile, GameObject playerS)
    {
        GameClient gc = Client.Instance.GetGameClientByName(Client.Instance.clientName);
        PlayerMovement pm = playerS.GetComponent<PlayerMovement>();
        switch (specialTile.GetSpecialTileType())
        {
            case SpecialTileType.GoToSchool:
                if (gc.myTurn) Client.Instance.Send("CToSchool|" + specialTile.name);

                break;

            case SpecialTileType.Bus:
                panel = specialTile.connectToPanelBus;
                if (gc.myTurn)
                {
                    panel.ShowPanel();
                }
                break;

            case SpecialTileType.BonusCard:
                if (gc.myTurn) Client.Instance.Send("CDrawCard|");
                break;

            case SpecialTileType.MinusCard:
                if (gc.myTurn) playerS.GetComponent<PlayerCards>().ChooseCardToDelete();

                break;

            case SpecialTileType.MinusChest:
                if (gc.myTurn) Client.Instance.Send("CMinusPoint|");
                break;

            case SpecialTileType.Stop:
                if (gc.myTurn) Client.Instance.Send("CBanned|");
                break;

            case SpecialTileType.Meteor:
                Client.Instance.Send("CMeteor|");
                break;


            default:
                break;
        }
    }

    public void OnClick()
    {
        Client.Instance.Send("CMoveBus|" + destinationTile.name);
        panel.HidePanel();

    }

    public void CheckAndMinusPoints(Area affectedArea, GameObject p)
    {
        
        PlayerMovement player  = p.GetComponent<PlayerMovement>();
        player.currentTile = GameManager.Instance.GetCurrentTile(p);
        if (player.currentTile.area == affectedArea)
        {
            if (player.score > 0)
            {
                Client.Instance.Send("CMinusPoint|");
            }

            Debug.Log("Player bị trừ điểm vì đứng ở khu vực bị Boom: " + affectedArea.name);
        }
        else
        {
            Debug.Log("Player không đứng ở khu vực bị Boom.");
        }
    }
    public IEnumerator SMoveToTile(Tile targetTile, PlayerMovement player)
    {
        Vector3 targetPosition = targetTile.transform.position + new Vector3(0f, 1f / 5, 0f);
        while (Vector3.Distance(player.transform.position, targetPosition) > 0.1f)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, player.moveSpeed * Time.deltaTime);
            yield return null;  // Chờ một frame
        }
        GameManager.Instance.currentTile = targetTile;
        yield return new WaitForSeconds(player.pauseDuration);
    }
}
