
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap Tilemap { get; private set; }
    
    // Tile references
    public Tile zeroTile;
    public Tile oneTile;
    public Tile twoTile;
    public Tile threeTile;
    public Tile fourTile;
    public Tile fiveTile;
    public Tile sixTile;
    public Tile sevenTile;
    public Tile eightTile;
    public Tile bombTile;
    public Tile explodingTile;
    public Tile hiddenTile;
    public Tile flagTile;

    private void Awake()
    {
        Tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        for (int i = 0; i < state.GetLength(0);i++)
        {
            for (int j = 0; j <  state.GetLength(1); j++)
            {
                Tilemap.SetTile(state[i, j].position, GetTile(state[i, j]));
            }
        }
    }
    
    private Tile GetTile(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {
            return flagTile;
        }
        else
        {
            return hiddenTile;
        }
    }

    private Tile GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case (Cell.Type.Empty):
                return zeroTile;
            case (Cell.Type.Mine):
                if (cell.exploded)
                {
                    return explodingTile;
                }
                else
                {
                    return bombTile;
                }
            case (Cell.Type.Number):
                return GetTileNumber(cell);
            default:
                return null;
        }
    }

    private Tile GetTileNumber(Cell cell)
    {
        switch (cell.number)
        {
            case 1:
                return oneTile;
            case 2:
                return twoTile;
            case 3:
                return threeTile;
            case 4:
                return fourTile;
            case 5:
                return fiveTile;
            case 6:
                return sixTile;
            case 7:
                return sevenTile;
            case 8:
                return eightTile;
            default:
                return null;
        }    
    }
}
