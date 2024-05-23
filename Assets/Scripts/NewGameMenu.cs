using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameMenu : MonoBehaviour
{

    public static bool GameIsPaused = false;
    public static bool InsideCustom = false;

    public GameObject NewGameMenuUI;
    public GameObject CustomMenuUI;

    private static int newGameRows = 9;
    private static int newGameCols = 9;
    private static int newGameMines = 10;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused && !InsideCustom)
            {
                Resume();
            }
            else if (GameIsPaused && InsideCustom)
            {
                LeaveCustomMenu();
            }
            else
            {
                Pause();
            }
        }
    }
    
    public void Resume()
    {
        NewGameMenuUI.SetActive(false);
        CustomMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        NewGameMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public static void Reset()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        InsideCustom = false;
    }

    public void StartNewEasy()
    {
        Debug.Log("Starting new Easy...");
        Reset();
        Logic.width = 9;
        Logic.height = 9;
        Logic.numMines = 10;
        SceneManager.LoadScene("Minesweeper");
    }

    public void StartNewMedium()
    {
        Debug.Log("Starting new Medium...");
        Reset();
        Logic.width = 16;
        Logic.height = 16;
        Logic.numMines = 64;
        SceneManager.LoadScene("Minesweeper");
    }

    public void StartNewHard()
    {
        Debug.Log("starting new Hard...");
        Reset();
        Logic.width = 32;
        Logic.height = 16;
        Logic.numMines = 150;
        SceneManager.LoadScene("Minesweeper");
    }

    public void OpenCustomMenu()
    {
        Debug.Log("Opening Custom settings...");
        CustomMenuUI.SetActive(true);
        InsideCustom = true;
    }

    public void LeaveCustomMenu()
    {
        CustomMenuUI.SetActive(false);
        InsideCustom = false;
    }

    public static void SetNewRows(int rows)
    {
        newGameRows = rows;
    }

    public static void SetNewCols(int cols)
    {
        newGameCols = cols;
    }

    public static void SetNewMines(int mines)
    {
        newGameMines = mines;
    }

    public void StartNewCustom()
    {
        Debug.Log("starting new Custom...");
        Reset();
        Logic.width = newGameCols;
        Logic.height = newGameRows;
        Logic.numMines = newGameMines;
        SceneManager.LoadScene("Minesweeper");
    }
}
