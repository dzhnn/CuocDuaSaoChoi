using System.Collections.Generic;
using UnityEngine;

public enum SpecialTileType
{
    None,       
    GoToSchool, 
    MinusCard,          
    BonusCard,  
    Bus,        
    AtSchool,   
    Stop,    
    Meteor,  
    MinusChest,  
    Revive,  
    Graduate, 
    ShopTiles 
}

public class Tile : MonoBehaviour
{
    public int tileID;  
    public List<Tile> neighbors;  
    public Tile targetTile; 
    public SpecialTileType specialTileType = SpecialTileType.None; 
    public Area area; 
    public Panel connectToPanelBus;

    private void OnMouseDown()
    {
        if (GameManager.Instance != null)
        {
            Client.Instance.Send("CTileClicked|" + this.name);
        }
    }

    public bool IsSpecialTile()
    {
        return specialTileType != SpecialTileType.None;
    }

    public SpecialTileType GetSpecialTileType()
    {
        if (specialTileType == SpecialTileType.None)
        {
            return SpecialTileType.None;
        }

        return specialTileType;
    }

    public Tile GetTargetTile()
    {
        if (this.IsSpecialTile()) return targetTile;
        return null;
    }

}
