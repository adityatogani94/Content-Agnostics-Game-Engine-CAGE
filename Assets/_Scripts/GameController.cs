using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    // Use this for initialization

    private GameObject[] hazards;
    private char[] hazardNums;

    public GameObject hazard;
    public Vector3 spawnValues;
    //public int hazardCount;
    public float spawnWait;
    public float startWait;
    public float waveWait;

    public GUIText scoreText;
    public GUIText restartText;
    public GUIText gameOverText;
    public GUIText questionText;

    private bool gameOver;
    private bool restart;
    private int score;

    void Update()
    {
        if(restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }
    void Start()
    {
        gameOver = false;
        restart = false;
        restartText.text = "";
        gameOverText.text = "";
        hazards = new GameObject[10];
        hazardNums = new char[hazards.Length];
        questionText.text = "";
        questionText.text = FrameworkCore.currentContent.getTerm();
        score = 0;
        UpdateScore();
        StartCoroutine(SpawnWaves());
    }
    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(startWait);
        
        for(int i = 0; i < hazards.Length; i++)
        {
            hazards[i] = hazard;
            hazardNums[i] = FrameworkCore.currentContent.getItem();
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnValues.x, spawnValues.x),
                spawnValues.y,
                spawnValues.z);
            Quaternion spawnRotation = Quaternion.identity;
            GameObject temp =(GameObject)Instantiate(hazards[i], spawnPosition, spawnRotation);
            temp.GetComponentInChildren<TextMesh>().text = "" + hazardNums[i];
            yield return new WaitForSeconds(spawnWait);

            if (gameOver)
            {
                restartText.text = "Press 'R' for restart";
                restart = true;
                break;
            }
        }
        //yield return new WaitForSeconds(waveWait);

        
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        UpdateScore(); 
    }

    void UpdateScore()
    {
        scoreText.text = "Score:" + score;
    }

    public void GameOver()
    {
        gameOverText.text = "Game Over!";
        gameOver = true;
    }

    public void winGame()
    {
        gameOverText.text = "You win!";
        gameOver = true;
    }
}

