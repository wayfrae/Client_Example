using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject hazard;
    public Vector3 spawnValues;

    private bool gameOver;
    private bool restart;
    private int score;

    private Canvas canvas;

    private void Start()
    {
        gameOver = false;
        restart = false;
        
        score = 0;
        
    }

    private void Update()
    {
        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    internal void SpawnAsteroids(List<Hazard> hazards)
    {        
            for (int i = 0; i < hazards.Count; i++)
            {                
                Vector3 spawnPosition = new Vector3(hazards[i].X, spawnValues.y, hazards[i].Y);
                Quaternion spawnRotation = Quaternion.identity;
                Instantiate(hazard, spawnPosition, spawnRotation);
            }            
    }

    public void AddScore(int scoreValue)
    {
        score += scoreValue;
        UpdateScore();
    }

    public void GameOver()
    {
        gameOver = true;
    }

    void UpdateScore()
    {
    }
}
