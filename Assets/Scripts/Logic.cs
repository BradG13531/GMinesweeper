using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class Logic : MonoBehaviour
{
    public static int width = 9;
    public static int height = 9;
    public static int numMines = 10;
    public int numFlags;

    public bool gameOver = false;

    // The number of cells that have not been revealed
    private int unrevealedCells;

    // Padding around camera to keep entire board in view
    public int cameraPadding = 1;
    
    // Tracks if the player has made their first move
    private bool firstMove = true;
    
    // Tracks if the player made a move so GameOver() can be called
    private bool playerClicked;

    private Board board;
    private Border border;
    public NewGameMenu menu;
    private Cell[,] state;

    public void SetWidth(int num)
    {
        width = num;
    }

    // #0: Very first method executed 
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
        border = GetComponentInChildren<Border>();
    }

    // #1: Called just before update for the first time
    private void Start()
    {
        NewGame();
    }

    // #2: Called just after start
    private void NewGame()
    {
        state = new Cell[width, height];
        
        unrevealedCells = width * height;
        numFlags = numMines;

        NewGameMenu.SetNewRows(9);
        NewGameMenu.SetNewCols(9);
        NewGameMenu.SetNewMines(10);
        
        AllHidden();

        Camera.main.transform.position = new Vector3((width) / 2f, (height + 2) / 2f, -10);
        // The orthographic size is the length from the middle to the edge of the camera,
        // the padding will give the extra unit of space at the edge.
        if (height + 2 >= width)
        {
            Camera.main.orthographicSize = (height + 3) / 2f + cameraPadding;
        }
        else
        {
            // float widthSize = (width / Camera.main.aspect) * 0.5f + cameraPadding;
            // while (widthSize < ((height + 3) / 2f + cameraPadding))
            // {
            //     widthSize++;
            // }
            float widthSize = width * Camera.main.aspect * 0.5f;
            if (widthSize > 10)
            {
                widthSize = 10;
            }

            Camera.main.orthographicSize = widthSize;
        }
        
        border.Draw(width, height, numFlags);
        board.Draw(state);
        
    }

    private void GameReset()
    {
        NewGameMenu.Reset();
        SceneManager.LoadScene("Minesweeper");
    }
    
    // #3: Called just after NewGame
    // Creates a board of all hidden mines so the player can click on their starting point
    private void AllHidden()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                state[i, j].position = new Vector3Int(i, j, 0);
                state[i, j].revealed = false;
            }
        }
    }

    
    // Creates the mines in random locations on the board
    private void CreateMines(int x, int y)
    {
        if (numMines >= height * width)
        {
            numMines = (int) Math.Round(height * width * 0.3);
        }
        Random randomX = new Random();
        Random randomY = new Random();

        int randomXCord = randomX.Next(0, width);
        int randomYCord = randomY.Next(0, height);

        int minesPlaced = 0;
        while (minesPlaced < numMines)
        {
            // Check if the random coordinate is either the starting cell or if it's already a mine
            if (!(randomXCord == x && randomYCord == y) && (state[randomXCord, randomYCord].type != Cell.Type.Mine))
            {
                state[randomXCord, randomYCord].type = Cell.Type.Mine;
                minesPlaced++;
            }

            randomXCord = randomX.Next(0, width);
            randomYCord = randomY.Next(0, height);
        }
        InitializeField();
    }
    

    // Called after mines are created, initializes the rest of the cells with numbers
    private void InitializeField()
    {
        // Outer two loops go through each individual cell
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (state[col, row].type != Cell.Type.Mine)
                {
                    int minesFound = 0;
                    // Iterates through adjacent 8 cells to count mines
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (IsValidCoordinates(col + i, row + j))
                            {
                                if (!(i == 0 && j == 0) && (state[col + i, row + j].type == Cell.Type.Mine))
                                {
                                    {
                                        minesFound++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Set number
                    if (minesFound == 0)
                    {
                        state[col, row].type = Cell.Type.Empty;
                    }
                    else
                    {
                        state[col, row].type = Cell.Type.Number;
                        state[col, row].number = minesFound;
                    }
                }
            }
        }
    }

    
    // Breadth-first search using queue
    private void StartingArea(int col, int row)
    {
        int[] origLocation = { col, row };
        Queue<int[]> queue = new Queue<int[]>();
        queue.Enqueue(origLocation);

        while (queue.Count != 0)
        {
            // Remove first element from queue
            int[] location = queue.Dequeue();

            // If mine, end search
            if (state[location[0], location[1]].type == Cell.Type.Mine)
            {
                break;
            }

            else
            {
                if (state[location[0], location[1]].type == Cell.Type.Empty)
                {
                    RevealZeroes(location[0], location[1]);             
                }
                else if (!(state[location[0], location[1]].revealed))
                {
                    state[location[0], location[1]].revealed = true;
                    unrevealedCells--;
                }
            }
            
            // Queue surrounding four directions
            if (IsValidCoordinates(location[0] - 1, location[1]) && !(state[location[0] - 1, location[1]].revealed))
            {
                int[] nextLocation = new[] { location[0] - 1, location[1] };
                queue.Enqueue(nextLocation);
            }

            if (IsValidCoordinates(location[0] + 1, location[1]) && !(state[location[0] + 1, location[1]].revealed))
            {
                int[] nextLocation = new[] { location[0] + 1, location[1] };
                queue.Enqueue(nextLocation);
            }

            if (IsValidCoordinates(location[0], location[1] - 1) && !(state[location[0], location[1] - 1].revealed))
            {
                int[] nextLocation = new[] { location[0], location[1] - 1 };
                queue.Enqueue(nextLocation);
            }

            if (IsValidCoordinates(location[0], location[1] + 1) && !(state[location[0], location[1] + 1].revealed))
            {
                int[] nextLocation = new[] { location[0], location[1] - 1 };
                queue.Enqueue(nextLocation);
            }
            
        }
    }
    
    
    // Depth-first search using stack
    private void RevealZeroes(int col, int row)
    {
        int[] origLocation = new[] { col, row };
        Stack<int[]> stack = new Stack<int[]>();
        stack.Push(origLocation);
        
        while (stack.Count != 0)
        {
            int[] location = stack.Pop();
        
            if (!(state[location[0], location[1]].revealed))
            {
                state[location[0], location[1]].revealed = true;
                unrevealedCells--;
            }
        
            // Reveal cells surrounding the current 0 that aren't also 0's.
            // This gives an outline to the pool of zeroes that form
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (IsValidCoordinates(location[0] + i, location[1] + j) && (state[location[0] + i,
                                                                                     location[1] + j].type !=
                                                                                 Cell.Type.Empty)
                                                                             && (state[location[0] + i,
                                                                                     location[1] + j].type !=
                                                                                 Cell.Type.Mine) && !(state[
                                                                                 location[0] + i,
                                                                                 location[1] + j].revealed)
                                                                             && !(i == 0 && j == 0))
                    {
                        state[location[0] + i, location[1] + j].revealed = true;
                        unrevealedCells--;
                    }
                }
            }
            // Left
            if (IsValidCoordinates(location[0] - 1, location[1]))
            {
                if (!(state[location[0] - 1, location[1]].revealed)
                    && (state[location[0] - 1, location[1]].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] - 1, location[1]].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] - 1, location[1] };
                    stack.Push(nextLocation);
                }
            }
        
            // Right
            if (IsValidCoordinates(location[0] + 1, location[1]))
            {
                if (!(state[location[0] + 1, location[1]].revealed)
                    && (state[location[0] + 1, location[1]].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] + 1, location[1]].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] + 1, location[1] };
                    stack.Push(nextLocation);
                }
            }
        
            // Down
            if (IsValidCoordinates(location[0], location[1] - 1))
            {
                if (!(state[location[0], location[1] - 1].revealed)
                    && (state[location[0], location[1] - 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0], location[1] - 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0], location[1] - 1 };
                    stack.Push(nextLocation);
                }
            }
        
            // Up
            if (IsValidCoordinates(location[0], location[1] + 1))
            {
                if (!(state[location[0], location[1] + 1].revealed)
                    && (state[location[0], location[1] + 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0], location[1] + 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0], location[1] + 1 };
                    stack.Push(nextLocation);
                }
            }
            // Down Left
            if (IsValidCoordinates(location[0] - 1, location[1] - 1))
            {
                if (!(state[location[0] - 1, location[1] - 1].revealed)
                    && (state[location[0] - 1, location[1] - 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] - 1, location[1] - 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] - 1, location[1] - 1 };
                    stack.Push(nextLocation);
                }
            }
            // Down Right
            if (IsValidCoordinates(location[0] + 1, location[1] - 1))
            {
                if (!(state[location[0] + 1, location[1] - 1].revealed)
                    && (state[location[0] + 1, location[1] - 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] + 1, location[1] - 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] + 1, location[1] - 1 };
                    stack.Push(nextLocation);
                }
            }
            // Up left
            if (IsValidCoordinates(location[0] - 1, location[1] + 1))
            {
                if (!(state[location[0] - 1, location[1] + 1].revealed)
                    && (state[location[0] - 1, location[1] + 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] - 1, location[1] + 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] - 1, location[1] + 1 };
                    stack.Push(nextLocation);
                }
            }
            // Up right
            if (IsValidCoordinates(location[0] + 1, location[1] + 1))
            {
                if (!(state[location[0] + 1, location[1] + 1].revealed)
                    && (state[location[0] + 1, location[1] + 1].type ==
                        Cell.Type.Empty) &&
                    (state[location[0] + 1, location[1] + 1].type !=
                     Cell.Type.Mine))
                {
                    int[] nextLocation = new[] { location[0] + 1, location[1] + 1 };
                    stack.Push(nextLocation);
                }
            }
        }
    }

    // If the user middle-clicks a revealed cell and flags are placed correctly, reveal
    // surrounding cells
    private void RevealSurrounding()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.Tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        
        
        if (gameOver || cell.type == Cell.Type.Invalid || cell.flagged || NewGameMenu.GameIsPaused)
        {
            return;
        }
        
        else if (cell.revealed)
        {
            int x = cellPosition.x;
            int y = cellPosition.y;
            
            int flaggedMines = 0;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (IsValidCoordinates(x + i, y + j))
                    {
                        if (state[x + i, y + j].flagged) 
                        {
                            flaggedMines++;
                        }
                    }
                }
            }

            if (flaggedMines != state[x, y].number) return;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (IsValidCoordinates(x + i, y + j))
                    {
                        if (!(state[x + i, y + j].flagged)) 
                        {
                            if (state[x + i, y + j].type == Cell.Type.Empty)
                            {
                                RevealZeroes(x + i, y + j);
                            }
                            else
                            {
                                if (state[x + i, y + j].type == Cell.Type.Mine)
                                {
                                    state[x + i, y + j].exploded = true;
                                }
                                else if (!(state[x + i, y + j].revealed))
                                {
                                    state[x + i, y + j].revealed = true;
                                    unrevealedCells--;
                                }
                            }
                        }
                    }
                }
            }
        }
        board.Draw(state);
    }

    // Checks for game over conditions
    private void GameOver()
    {
        bool mineExloded = false;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Mine && state[i, j].revealed)
                {
                    mineExloded = true;
                    break;
                }
            }
        }

        if (mineExloded && !gameOver)
        {
            Debug.Log("Game Over");
            gameOver = true;
            border.ToggleTimer();
            RevealAll();
            border.SetDead();
            board.Draw(state);
        }
        else if (unrevealedCells == numMines && !gameOver)
        {
            Debug.Log("You won!");
            gameOver = true;
            border.ToggleTimer();
            FlagRemainingMines();
            border.SetCool();
            board.Draw(state);
        }
    }
    
    // Reveals all cells, called when a bomb explodes
    private void RevealAll()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                state[i, j].revealed = true;
            }
        }
    }

    // Only called if the user has revealed all non-mine cells
    private void FlagRemainingMines()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Mine)
                {
                    state[i, j].flagged = true;
                }
            }
        } 
    }
    

    private void Update()
    {
        // Reveal and other buttons
        if (Input.GetMouseButtonDown(0))
        {
            Reveal();
            playerClicked = true;
        }
        // Flag
        else if (Input.GetMouseButtonDown(1))
        {
            Flag();
            playerClicked = true;
        }
        // Reveal surrounding
        else if (Input.GetMouseButtonDown(2))
        {
            RevealSurrounding();
            playerClicked = true;
        }

        if (playerClicked)
        {
           GameOver();
           playerClicked = false;
        }
    }
    
    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.Tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (firstMove && IsValidCoordinates(cellPosition.x, cellPosition.y) && !NewGameMenu.GameIsPaused)
        {
            CreateMines(cellPosition.x, cellPosition.y);
            StartingArea(cellPosition.x, cellPosition.y);
            firstMove = false;
        }
        else if (gameOver || cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged || NewGameMenu.GameIsPaused)
        {
            
            if (cellPosition.x == border.SmileyLocation.x && cellPosition.y == border.SmileyLocation.y)
            {
                GameReset();
            }
            else if (cellPosition.x == border.MenuLocation.x && cellPosition.y == border.MenuLocation.y)
            {
                if (NewGameMenu.GameIsPaused)
                {
                    
                    menu.Resume();
                }
                else
                {
                    menu.Pause();
                }
            }
            else if (cellPosition.x == border.QuitLocation.x && cellPosition.y == border.QuitLocation.y)
            {
                Debug.Log("quit");
                Application.Quit();
            }
            return;
        }
        else if (cell.type == Cell.Type.Mine)
        {
            state[cellPosition.x, cellPosition.y].revealed = true;
            state[cellPosition.x, cellPosition.y].exploded = true;
            GameOver();
        }
        else
        {
            if (state[cellPosition.x, cellPosition.y].type == Cell.Type.Empty)
            {
                RevealZeroes(cellPosition.x, cellPosition.y);
            }
            else
            { 
                cell.revealed = true; 
                state[cellPosition.x, cellPosition.y] = cell;
                unrevealedCells--;
            }
        }
        
        board.Draw(state);
    }
    
    
    // Adds and removes flags from cells
    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.Tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        // First move and game not paused
        if (firstMove && !NewGameMenu.GameIsPaused)
        {
            cell.flagged = !cell.flagged;

            state[cellPosition.x, cellPosition.y] = cell;
        }
        
        // Invalid State, can't put flag down
        else if (gameOver || cell.type == Cell.Type.Invalid || cell.revealed || NewGameMenu.GameIsPaused)
        {
            return;
        }
        // Player wants to remove a flag when there are 0 flags left
        else if (cell.flagged && numFlags == 0)
        {
            cell.flagged = !cell.flagged;
            state[cellPosition.x, cellPosition.y] = cell;
        }
        // Player tries to place a flag when there are 0 flags left
        else if (!cell.flagged && numFlags == 0)
        {
            return;
        }
        else
        { 
            cell.flagged = !cell.flagged;
            state[cellPosition.x, cellPosition.y] = cell;
        }
        
        if (cell.flagged)
        {
            numFlags--;
            border.UpdateFlagCounter(height, numFlags);
        }
        else
        {
            numFlags++;
            border.UpdateFlagCounter(height, numFlags);
        }
        board.Draw(state);
    }
    
    private Cell GetCell(int x, int y)
    {
        if (IsValidCoordinates(x, y))
        {
            return state[x, y];
        }
        else
        {
            // Returns new cell that initializes to 'Invalid' type
            return new Cell();
        }
    }

    private bool IsValidCoordinates(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private bool PlayerClickedOutside(int x, int y)
    {
        if ((x < 0 || x >= width) && (y < 0 || y >= height))
        {
            return true;
        }
        else
        {
            return false;
        }
            
    }
}
