using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public List<Tile> GetPath(Tile startTile, Tile endTile, int steps)
    {
        List<Tile> currentPath = new List<Tile>();  
        HashSet<Tile> visited = new HashSet<Tile>();

        if (DFS(startTile, endTile, visited, currentPath, steps))
        {
            return currentPath;
        }
        else
        {
            return new List<Tile>(); 
        }
    }

    private bool DFS(Tile currentTile, Tile endTile, HashSet<Tile> visited, List<Tile> currentPath, int remainingSteps)
    {
        if (remainingSteps < 0) return false;
        if (currentTile == endTile)
        {
            currentPath.Add(currentTile);  
            return true;
        }

        visited.Add(currentTile);
        currentPath.Add(currentTile); 
        foreach (Tile neighbor in currentTile.neighbors)
        {
            if (!visited.Contains(neighbor))
            {
                if (DFS(neighbor, endTile, visited, currentPath, remainingSteps - 1))
                {
                    return true;  
                }
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);  
        return false;
    }

    public List<Tile> GetPossibleDestinations(Tile currentTile, int steps)
    {
        List<Tile> possibleDestinations = new List<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();
        FindPossibleDestinations(currentTile, steps, possibleDestinations, visited);

        return possibleDestinations;
    }

    private void FindPossibleDestinations(Tile currentTile, int remainingSteps, List<Tile> possibleDestinations, HashSet<Tile> visited)
    {
        if (remainingSteps == 0)
        {
            if (!possibleDestinations.Contains(currentTile))
            {
                possibleDestinations.Add(currentTile);
            }
            return;
        }

        visited.Add(currentTile); 

        foreach (Tile neighbor in currentTile.neighbors)
        {
            if (visited.Contains(neighbor)) continue; 

            FindPossibleDestinations(neighbor, remainingSteps - 1, possibleDestinations, visited);
        }
    }
}
