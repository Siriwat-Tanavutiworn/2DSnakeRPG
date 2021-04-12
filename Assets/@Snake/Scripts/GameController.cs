using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : StateLookUp
{
    public Vector2 xBorder;
    public Vector2 yBorder;

    public Fighting fightingSystem;
    [SerializeField] GameObject startText;
    [SerializeField] GameObject pauseBox;
    public bool isPause = false;

    [Header("Lose Canvas")]
    [SerializeField] GameObject LoseBox;
    [SerializeField] Text finalScore;

    [Header("Score")]
    public Text scoreText;
    public int score;

    // Start is called before the first frame update
    void Start()
    {
        state = GameState.Start;
        score = 0;
        pauseBox.gameObject.SetActive(false);
        fightingSystem.gameObject.SetActive(false);
        gameStates[GameState.Start] = () =>
        {

        };

        gameStates[GameState.Play] = () =>
        {
            startText.SetActive(false);
        };

        gameStates[GameState.Fight] = () =>
        {

        };

        gameStates[GameState.End] = () =>
        {
            EnableLoseBox();
        };
    }

    // Update is called once per frame
    void Update()
    {
        gameStates[state]();
        scoreText.text = "Score : " + score.ToString();
        CheckPauseGame();
    }

    public void CallFighting(PlayerController mainPly, EnemyController enim)
    {
        state = GameState.Fight;
        fightingSystem.gameObject.SetActive(true);
        fightingSystem.CallFighting(mainPly, enim);
    }

    public void IncreaseScore(int value)
    {
        score += value;
    }

    void EnableLoseBox()
    {
        LoseBox.SetActive(true);
        finalScore.text = "Your Score : " + score;
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
        Time.timeScale = 1;

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void CheckPauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = !isPause;

            if (isPause)
            {
                Time.timeScale = 0;
                pauseBox.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                pauseBox.SetActive(false);
            }
        }
    }

    public void Continue()
    {
        isPause = !isPause;
        Time.timeScale = 1;
        pauseBox.SetActive(false);
    }
}
