using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : StateLookUp
{
    GameController controller;

    [Header("Follower")]
    //[SerializeField] PlayerController followerPrefeb;
    [SerializeField] GameObject playerGroup;
    [SerializeField] List<PlayerController> allHeroSpawn = new List<PlayerController>();
    [SerializeField] float maxFollowerTimer = 10f;
    [SerializeField] float currentFollowerTimer;

    [Header("Enemy")]
    //[SerializeField] EnemyController enemyPrefeb;
    [SerializeField] GameObject enemyGroup;
    [SerializeField] List<EnemyController> allEnemySpawn = new List<EnemyController>();
    [SerializeField] float maxEnemyTimer = 10f;
    [SerializeField] float currentEnemyTimer;
    [SerializeField] int maxEnemyInLine = 3;

    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<GameController>();
        currentFollowerTimer = maxFollowerTimer;
        currentEnemyTimer = maxEnemyTimer;

        if (maxEnemyInLine < 1) maxEnemyInLine = 1;

        gameStates[GameState.Start] = () =>
        {

        };

        gameStates[GameState.Play] = () =>
        {
            SpawnFollower();
            SpawnEnemy();
        };

        gameStates[GameState.Fight] = () =>
        {

        };

        gameStates[GameState.End] = () =>
        {

        };
    }

    // Update is called once per frame
    void Update()
    {
        gameStates[state]();
    }

    void SpawnFollower()
    {
        currentFollowerTimer -= Time.deltaTime;
        if(currentFollowerTimer <= 0)
        {
            int randX = (int)Random.Range(controller.xBorder.x, controller.xBorder.y);
            int randY = (int)Random.Range(controller.yBorder.x, controller.yBorder.y);

            Vector3 spawnPoint = new Vector3(randX, randY, 0);

            RaycastHit2D hit = Physics2D.Raycast(new Vector3(spawnPoint.x, spawnPoint.y, -10), Vector3.forward, Mathf.Infinity);

            while(hit.collider != null)
            {
                randX = (int)Random.Range(controller.xBorder.x, controller.xBorder.y);
                randY = (int)Random.Range(controller.yBorder.x, controller.yBorder.y);
                spawnPoint = new Vector3(randX, randY, 0);

                hit = Physics2D.Raycast(new Vector3(spawnPoint.x, spawnPoint.y, -10), Vector3.forward, Mathf.Infinity);
            }

            int rand = Random.Range(0, allHeroSpawn.Count);

            GameObject followerTemp = Instantiate(allHeroSpawn[rand].gameObject, spawnPoint, Quaternion.identity, playerGroup.transform);

            PlayerController followerController = followerTemp.GetComponent<PlayerController>();
            followerController.type = MainUnitType.FollowerPicking;

            currentFollowerTimer = maxFollowerTimer;
        }
    }

    void SpawnEnemy()
    {
        currentEnemyTimer -= Time.deltaTime;
        if (currentEnemyTimer <= 0)
        {
            EnemyController mainEnemy = null;

            int randCount = Random.Range(1, maxEnemyInLine);

            int randX = (int)Random.Range(controller.xBorder.x, controller.xBorder.y);
            int randY = (int)Random.Range(controller.yBorder.x, controller.yBorder.y);

            Vector3 spawnPoint = new Vector3(randX, randY, 0);

            RaycastHit2D hit = Physics2D.Raycast(new Vector3(spawnPoint.x, spawnPoint.y, -10), Vector3.forward, Mathf.Infinity);

            while (hit.collider != null)
            {
                randX = (int)Random.Range(controller.xBorder.x, controller.xBorder.y);
                randY = (int)Random.Range(controller.yBorder.x, controller.yBorder.y);
                spawnPoint = new Vector3(randX, randY, 0);

                hit = Physics2D.Raycast(new Vector3(spawnPoint.x, spawnPoint.y, -10), Vector3.forward, Mathf.Infinity);
            }

            for (int i = 0; i < randCount; i++)
            {
                int rand = Random.Range(0, allEnemySpawn.Count);

                GameObject enemyTemp;
                EnemyController enemyController;
                if (i == 0)
                {
                    enemyTemp = Instantiate(allEnemySpawn[rand].gameObject, spawnPoint, Quaternion.identity, enemyGroup.transform);

                    enemyController = enemyTemp.GetComponent<EnemyController>();
                    enemyController.type = EnemyType.MainEnemy;
                    enemyController.allLineUnit.Add(enemyController);
                    mainEnemy = enemyController;
                    enemyController.InitDirection();
                }
                else
                {
                    enemyTemp = Instantiate(allEnemySpawn[rand].gameObject, spawnPoint, Quaternion.identity, enemyGroup.transform);

                    enemyController = enemyTemp.GetComponent<EnemyController>();
                    enemyController.type = EnemyType.EnemyFollower;
                    mainEnemy.allLineUnit.Add(enemyController);

                    enemyTemp.transform.position = mainEnemy.allLineUnit[i - 1].startPoint - mainEnemy.allLineUnit[i - i].dir;

                    enemyController.startPoint = enemyTemp.transform.position;
                    enemyController.endPoint = enemyTemp.transform.position + mainEnemy.allLineUnit[i - i].dir;
                }
            }

            for (int i = 1; i < mainEnemy.allLineUnit.Count; i++)
            {
                mainEnemy.allLineUnit[i].allLineUnit = mainEnemy.allLineUnit;
            }

            currentEnemyTimer = maxEnemyTimer;
        }
    }
}
