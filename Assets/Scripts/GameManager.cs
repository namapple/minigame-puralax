using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Constants;

public class GameManager : MonoBehaviour
{
    #region VARIABLES

    public static GameManager instance;

    [SerializeField] private TMP_Text stageText, levelText;
    [SerializeField] private Image titleImage, winImage;
    [SerializeField] private AudioClip moveClip, updateClip, winClip, loseClip;
    
    public List<Color> colors;
    [SerializeField] private LevelDictionary levelDictionary;
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Button btnBack, btnRestart;

    private Dictionary<Vector2Int, Cell> cellDictionary = new Dictionary<Vector2Int, Cell>();

    private Level currentLevelObject;
    private string currentLevelName;
    private int winColor;
    private GameState currentGameState;
    private Vector2Int startClickGrid, endClickGrid;
    private float stateDelay;

    private List<Cell> neighbours = new List<Cell>();
    private List<Cell> newNeighbours = new List<Cell>();
    private Dictionary<Vector2Int, bool> visited = new Dictionary<Vector2Int, bool>();

    private readonly List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    #endregion

    #region GAMEOBJECT_METHODS

    private void Awake()
    {
        instance = this;

        stageText.text = "STAGE " + PlayerPrefs.GetInt(DATA.CURRENT_STAGE);
        levelText.text = "Level " + PlayerPrefs.GetInt(DATA.CURRENT_LEVEL);

        winImage.gameObject.SetActive(false);
        currentGameState = GameState.INPUT;
        SpawnLevel();

        AudioManager.instance.AddButtonSound();
    }

    private void Update()
    {
        if(currentGameState != GameState.INPUT) return;

        Vector3 inputPos;
        Vector2Int currentClickedPos;

        if (Input.GetMouseButtonDown(0))
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentClickedPos = new Vector2Int((int) inputPos.x, (int) inputPos.y);
            startClickGrid = currentClickedPos;
        }
        
        else if (Input.GetMouseButtonUp(0))
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentClickedPos = new Vector2Int((int) inputPos.x, (int) inputPos.y);
            endClickGrid = currentClickedPos;
            endClickGrid = GetDirection(endClickGrid - startClickGrid);

            Debug.Log("STARTPOS : " + startClickGrid);
            Debug.Log("OFFSET : " + endClickGrid);
            currentGameState = GameState.ANIMATION;
            CalculateMoves();
        }
    }
    

    #endregion

    #region SPAWNER

    private void SpawnLevel()
    {
        int currentStage = PlayerPrefs.GetInt(DATA.CURRENT_STAGE);
        string currentStageName = DATA.CURRENT_STAGE + "_" + currentStage;
        int currentLevel = PlayerPrefs.GetInt(DATA.CURRENT_LEVEL);
        currentLevelName = currentStageName + "_" + currentLevel;

        currentLevelObject = levelDictionary.GetLevel(currentLevelName);
        winColor = currentLevelObject.winColor;

        //SPAWN ALL CELLS
        for (int i = 0; i < currentLevelObject.row; i++)
        {
            for (int j = 0; j < currentLevelObject.col; j++)
            {
                Vector3 spawnPos = new Vector3(i + 0.5f, j + 0.5f);
                Cell temp = Instantiate(cellPrefab, spawnPos, Quaternion.identity);
                cellDictionary[new Vector2Int(i, j)] = temp;
            }
        }
        
        //SPAWN COLORED CELLS
        foreach (var item in currentLevelObject.cellData)
        {
            cellDictionary[item.gridPos].InitCell(item);
        }
        
        //SET UP CAMERRA
        float size = 0f;
        if (currentLevelObject.col <= currentLevelObject.row)
        {
            size = (currentLevelObject.row * 0.5f) + 2.5f;
        }
        else
        {
            size = (currentLevelObject.col * 0.5f) + 3.5f;
        }

        Camera.main.orthographicSize = size;
        Camera.main.transform.Translate(currentLevelObject.row * 0.5f,
            currentLevelObject.col * 0.5f, 0);
        // CHANGE TITLE COLOR
        titleImage.color = colors[winColor];
    }

    #endregion

    private void Start()
    {
        btnBack.onClick.AddListener(BackToMenu);
        btnRestart.onClick.AddListener(GameRestart);
    }

    #region GAME_FUNCTIONS

    private void GameWin()
    {
        int currentStage = PlayerPrefs.GetInt(DATA.CURRENT_STAGE);
        string currentStageName = DATA.CURRENT_STAGE + "_" + currentStage;
        int currentLevel = PlayerPrefs.GetInt(DATA.CURRENT_LEVEL);

        // SET THE LEVEL TO WON
        PlayerPrefs.SetInt(currentLevelName, 2);
        
        // UNLOCK THE NEXT LEVEL
        int updateLevel = currentLevel + 5;
        if (updateLevel <= 20)
        {
            PlayerPrefs.SetInt(currentStageName + "_" + updateLevel, 1);
        }
        else
        {
            int updateStage = currentStage + 1;
            PlayerPrefs.SetInt(DATA.CURRENT_STAGE + "_" + updateStage, 1);
        }
        
        // SET THE CURRENT LEVEL
        int playLevel = currentLevel + 1;
        if (playLevel > 20)
        {
            currentStage++;
            playLevel = 1;
        }
        
        PlayerPrefs.SetInt(DATA.CURRENT_STAGE, currentStage);
        PlayerPrefs.SetInt(DATA.CURRENT_LEVEL, playLevel);

        SceneManager.LoadScene(1);
    }

    private void GameLose()
    {
        SceneManager.LoadScene(1);
    }

    private void ShowWin()
    {
        winImage.gameObject.SetActive(true);
        winImage.color = colors[winColor];
        AudioManager.instance.PlaySound(winClip);
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion

    #region MOVES
    private void CalculateMoves()
    {
        AudioManager.instance.PlaySound(moveClip);
        
        //Valid startpos
        if (!IsValidPos(startClickGrid))
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }

        Cell currentClickedCell = cellDictionary[startClickGrid];
        
        // VALID ENDPOS AND HAS MOVES
        if (!IsValidPos(startClickGrid + endClickGrid) || !(currentClickedCell.cellData.moves > 0))
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }
        
        // INVALID SAME COLOR
        Cell endClickedCell = cellDictionary[startClickGrid + endClickGrid];

        if (currentClickedCell.cellData.color == endClickedCell.cellData.color)
        {
            stateDelay = 0f;
            StartCoroutine(SwitchStateAfterDelay());
            return;
        }
        
        // MOVE FOR EMPTY CELL

        if (endClickedCell.cellData.color == -1)
        {
            currentClickedCell.cellData.moves -= 1;
            StartCoroutine(currentClickedCell.UpdateMoves());

            var temp = endClickedCell.cellData.gridPos;
            endClickedCell.cellData.gridPos = currentClickedCell.cellData.gridPos;
            currentClickedCell.cellData.gridPos = temp;

            StartCoroutine(currentClickedCell.MoveToPos());
            StartCoroutine(endClickedCell.MoveToPos());

            cellDictionary[startClickGrid] = endClickedCell;
            cellDictionary[startClickGrid + endClickGrid] = currentClickedCell;

            stateDelay = VALUES.ANIMATION_TIME;
            StartCoroutine(SwitchStateAfterDelay());

            CheckResult();
            return;
        }
        
        // UPDATE THE FIRST COLLIED CELL
        int updateColor = endClickedCell.cellData.color;
        endClickedCell.cellData.color = currentClickedCell.cellData.color;

        StartCoroutine(endClickedCell.ChangeColor(0f));
        currentClickedCell.cellData.moves--;

        stateDelay = VALUES.ANIMATION_TIME;
        StartCoroutine(currentClickedCell.UpdateMoves());
        StartCoroutine(SwitchStateAfterDelay());
        
        // Check for neighbours cells
        newNeighbours.Clear();
        neighbours.Clear();
        visited.Clear();
        neighbours.Add(endClickedCell);

        while (neighbours.Count>0)
        {
            newNeighbours.Clear();
            for (int i = 0; i < neighbours.Count; i++)
            {
                for (int j = 0; j < directions.Count; j++)
                {
                    if (IsValidPos(neighbours[i].CurrentPos + directions[j]))
                    {
                        endClickedCell = cellDictionary[neighbours[i].CurrentPos + directions[j]];
                        if (!visited.ContainsKey(endClickedCell.CurrentPos))
                        {
                            if (endClickedCell.cellData.color == updateColor)
                            {
                                endClickedCell.cellData.color = currentClickedCell.cellData.color;
                                StartCoroutine(endClickedCell.ChangeColor(stateDelay));
                                newNeighbours.Add(endClickedCell);

                                visited[endClickedCell.CurrentPos] = true;
                            }
                        }
                    }
                }
            }
            
            Invoke(nameof(PlayUpdateSound), stateDelay);
            stateDelay += (newNeighbours.Count > 0 ? VALUES.ANIMATION_TIME : 0);
            neighbours.Clear();
            foreach (var item in newNeighbours)
            {
                neighbours.Add(item);
            }
        }

        CheckResult();

    }

    private void CheckResult()
    {
        int lose = 0;
        bool win = true;

        foreach (var item in cellDictionary)
        {
            lose += item.Value.cellData.moves;
            win = win && (item.Value.cellData.color == -1 || item.Value.cellData.color == winColor);
            
        }

        if (win)
        {
            Invoke(nameof(ShowWin), stateDelay + 0.5f);
            Invoke(nameof(GameWin), stateDelay + 1.5f);
            return;
        }
        else if(lose == 0)
        {
            AudioManager.instance.PlaySound(loseClip);
            Invoke(nameof(GameLose), stateDelay + 1f);
            return;
        }
    }
    #endregion

    #region HELPER_FUNCTION

    private Vector2Int GetDirection(Vector2Int offset)
    {
        Vector2Int result;
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            result = new Vector2Int(offset.x > 0 ? 1 : -1, 0);
        }
        else
        {
            result = new Vector2Int(0, offset.y > 0 ? 1 : -1);
        }

        return result;
    }

    public bool IsValidPos(Vector2Int pos)
    {
        return !(pos.x >= currentLevelObject.row || pos.x < 0 || pos.y < 0 ||
                pos.y >= currentLevelObject.col);
    }

    public void PlayUpdateSound()
    {
        AudioManager.instance.PlaySound(updateClip);
    }

    private IEnumerator SwitchStateAfterDelay()
    {
        while (stateDelay > 0f)
        {
            stateDelay -= Time.deltaTime;
            yield return null;
        }

        currentGameState = GameState.INPUT;
    }
    
    #endregion
}

public enum GameState
{
    INPUT,
    ANIMATION
}