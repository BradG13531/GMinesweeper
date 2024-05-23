using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Border : MonoBehaviour
{
    public Tilemap Tilemap { get; private set; }
    
    // References to border tiles
    public Tile verticalTile;
    public Tile horizontalTile;
    public Tile bottomLeftTile;
    public Tile bottomRightTile;
    public Tile topLeftTile;
    public Tile topRightTile;
    public Tile threeLaneLeftTile;
    public Tile threeLaneRightTile;
    public Tile greyTile;
    
    // References to face tiles
    public Tile smileyTile;
    public Tile intriguedTile;
    public Tile deadTile;
    public Tile menuTile;
    public Tile quitTile;
    public Tile coolTile;
    
    // References to seven segment display tiles
    public Tile displayEmpty;
    public Tile displayZero;
    public Tile displayOne;
    public Tile displayTwo;
    public Tile displayThree;
    public Tile displayFour;
    public Tile displayFive;
    public Tile displaySix;
    public Tile displaySeven;
    public Tile displayEight;
    public Tile displayNine;

    public Vector3Int SmileyLocation;
    public Vector3Int MenuLocation;
    public Vector3Int QuitLocation;
    
    private int width;
    private int height;

    private float timer = 0f;
    public bool pauseTimer = false;

    private void Awake()
    {
        Tilemap = GetComponent<Tilemap>();
    }

    public void Draw(int width, int height, int flags)
    {
        this.width = width;
        this.height = height;
        Tilemap.SetTile(new Vector3Int(-1, -1), bottomLeftTile);
        Tilemap.SetTile(new Vector3Int(width, -1), bottomRightTile);
        Tilemap.SetTile(new Vector3Int(-1, height), threeLaneLeftTile);
        Tilemap.SetTile(new Vector3Int(width, height), threeLaneRightTile);
        Tilemap.SetTile(new Vector3Int(-1, height + 1), verticalTile);
        Tilemap.SetTile(new Vector3Int(width, height + 1), verticalTile);
        Tilemap.SetTile(new Vector3Int(-1, height + 2), topLeftTile);
        Tilemap.SetTile(new Vector3Int(width, height + 2), topRightTile);
        

        // Bottom row 
        for (int i = 0; i < width; i++)
        {
            Tilemap.SetTile(new Vector3Int(i, -1), horizontalTile);
            Tilemap.SetTile(new Vector3Int(i, height), horizontalTile);
            Tilemap.SetTile(new Vector3Int(i, height + 2), horizontalTile);
        }
        // Columns
        for (int i = 0; i < height; i++)
        {
            Tilemap.SetTile(new Vector3Int(-1, i), verticalTile);
            Tilemap.SetTile(new Vector3Int(width, i), verticalTile);
        }

        Tile firstDigit = GetSevenSegment(flags % 10);
        flags /= 10;
        Tile secondDigit = GetSevenSegment(flags % 10);
        flags /= 10;
        Tile thirdDigit = GetSevenSegment(flags % 10);

        Tilemap.SetTile(new Vector3Int(0, height + 1), thirdDigit);
        Tilemap.SetTile(new Vector3Int(1, height + 1), secondDigit);
        Tilemap.SetTile(new Vector3Int(2, height + 1), firstDigit);
        if ((width / 2) - 3 > 0)
        {
            int x = 3;
            while (width / 2 - 1 - x > 0)
            {
                Tilemap.SetTile(new Vector3Int(x, height + 1), greyTile);
                x++;
            }
        }

        MenuLocation = new Vector3Int(width / 2 - 1, height + 1);
        Tilemap.SetTile(MenuLocation, menuTile);
        SmileyLocation = new Vector3Int(width / 2, height + 1);
        Tilemap.SetTile(SmileyLocation, smileyTile);
        QuitLocation = new Vector3Int(width / 2 + 1, height + 1);
        Tilemap.SetTile(QuitLocation, quitTile);
        
        if (width - 3 > (width / 2 + 2))
        {
            int x = width / 2 + 2;
            while (width - 3 > x)
            {
                Tilemap.SetTile(new Vector3Int(x, height + 1), greyTile);
                x++;
            }
        }
        Tilemap.SetTile(new Vector3Int(width - 3, height + 1), displayZero);
        Tilemap.SetTile(new Vector3Int(width - 2, height + 1), displayZero);
        Tilemap.SetTile(new Vector3Int(width - 1, height + 1), displayZero);
    }

    public void UpdateFlagCounter(int height, int numFlags)
    {
        Tile firstDigit = GetSevenSegment(numFlags % 10);
        numFlags /= 10;
        Tile secondDigit = GetSevenSegment(numFlags% 10);
        numFlags /= 10;
        Tile thirdDigit = GetSevenSegment(numFlags % 10);

        Tilemap.SetTile(new Vector3Int(0, height + 1), thirdDigit);
        Tilemap.SetTile(new Vector3Int(1, height + 1), secondDigit);
        Tilemap.SetTile(new Vector3Int(2, height + 1), firstDigit);
    }

    private Tile GetSevenSegment(int num)
    {
        switch (num)
        {
            case 0:
                return displayZero; 
            case 1:
                return displayOne;
            case 2:
                return displayTwo;
            case 3:
                return displayThree;
            case 4:
                return displayFour;
            case 5:
                return displayFive;
            case 6:
                return displaySix;
            case 7:
                return displaySeven;
            case 8:
                return displayEight;
            case 9:
                return displayNine;
            default:
                return displayEmpty;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        int oldTime = (int) timer;
        timer += Time.deltaTime;
        if ((int) timer > oldTime && (int) timer < 1000 && !pauseTimer)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        int seconds = Mathf.FloorToInt(timer);
        
        Tile firstDigit = GetSevenSegment(seconds % 10);
        seconds /= 10;
        Tile secondDigit = GetSevenSegment(seconds % 10);
        seconds /= 10;
        Tile thirdDigit = GetSevenSegment(seconds % 10);
        
        Tilemap.SetTile(new Vector3Int(width - 3, height + 1), thirdDigit);
        Tilemap.SetTile(new Vector3Int(width - 2, height + 1), secondDigit);
        Tilemap.SetTile(new Vector3Int(width - 1, height + 1), firstDigit);
    }

    public void ToggleTimer()
    {
        pauseTimer = !pauseTimer;
    }

    public void SetDead()
    {
        Tilemap.SetTile(SmileyLocation, deadTile);
    }

    public void SetCool()
    {
        Tilemap.SetTile(SmileyLocation, coolTile);
    }
}
