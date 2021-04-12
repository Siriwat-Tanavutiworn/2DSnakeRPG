using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class Fighting : StateLookUp
{
    GameController controller;
    [Header("Ending")]
    [SerializeField] GameObject WinCanvas;
    [SerializeField] GameObject LoseCanvas;
    [SerializeField] Text endScoreText;
    [SerializeField] Button restartBtn;
    [SerializeField] Button continueBtn;

    [Header("Predestal")]
    [SerializeField] GameObject playerPredestal;
    [SerializeField] GameObject enemyPredestal;

    [Header("Participate")]
    [SerializeField] PlayerController mainPlayer;
    [SerializeField] EnemyController mainEnemy;
    [SerializeField] PlayerController currentFightPlayer;
    [SerializeField] EnemyController currentFightEnemy;

    [Header("HealthController")]
    [SerializeField] Image playerHealth;
    [SerializeField] Image enemyHealth;
    [SerializeField] Text playerHealthText;
    [SerializeField] Text enemyHealthText;
    [SerializeField] GameObject damageText;
    int playerCount;
    int currentPlayer = 0;

    int enemyCount;
    int currentEnemy = 0;

    bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<GameController>();

        gameStates[GameState.Start] = () =>
        {

        };

        gameStates[GameState.Play] = () =>
        {

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
        
    }

    public void CallFighting(PlayerController ply, EnemyController enem)
    {
        if (!isActive)
        {
            isActive = true;
            WinCanvas.SetActive(false);
            LoseCanvas.SetActive(false);

            currentFightPlayer = null;
            currentFightEnemy = null;

            mainPlayer = ply;
            mainEnemy = enem;

            playerCount = ply.allLineUnit.Count;
            enemyCount = enem.allLineUnit.Count;
            currentPlayer = 0;
            currentEnemy = 0;

            FindObjectOfType<Sound_Manager>().Play_SelectSound("fighting");

            SpawnPlayer(ply, currentPlayer);
            SpawnEnemy(enem, currentEnemy);


            StartCoroutine(StartFight(currentFightPlayer, currentFightEnemy));
        }

    }

    private void SpawnPlayer(PlayerController ply, int index)
    {
        GameObject playerTemp = Instantiate(ply.allLineUnit[index].gameObject, playerPredestal.transform.position, Quaternion.identity);

        playerTemp.GetComponent<Animator>().Play("PIdleFight");

        playerTemp.GetComponent<PlayerController>().body.transform.localScale = Vector3.one;
        //playerTemp.GetComponentInChildren<SortingGroup>().sortingLayerName = "UI";
        //playerTemp.GetComponentInChildren<SortingGroup>().sortingOrder = 2;

        playerTemp.GetComponent<PlayerController>().body.GetComponent<SortingGroup>().sortingLayerName = "UI";
        playerTemp.GetComponent<PlayerController>().body.GetComponent<SortingGroup>().sortingOrder = 2;

        playerTemp.GetComponent<PlayerController>()._HP = ply.allLineUnit[index]._HP;

        currentFightPlayer = playerTemp.GetComponent<PlayerController>();
        playerHealth.fillAmount = currentFightPlayer._HP / currentFightPlayer.unitData._HP;
        playerHealthText.text = currentFightPlayer._HP + "/" + currentFightPlayer.unitData._HP;
    }

    private void SpawnEnemy(EnemyController enemy, int index)
    {
        GameObject enemyTemp = Instantiate(enemy.allLineUnit[index].gameObject, enemyPredestal.transform.position, Quaternion.identity);

        enemyTemp.GetComponent<Animator>().Play("EIdleAttack");

        enemyTemp.GetComponent<EnemyController>().body.transform.localScale = Vector3.one;

        enemyTemp.GetComponentInChildren<SortingGroup>().sortingLayerName = "UI";
        enemyTemp.GetComponentInChildren<SortingGroup>().sortingOrder = 2;
        currentFightEnemy = enemyTemp.GetComponent<EnemyController>();
        enemyHealth.fillAmount = currentFightEnemy._HP / currentFightEnemy.unitData._HP;
        enemyHealthText.text = currentFightEnemy._HP + "/" + currentFightEnemy.unitData._HP;
    }


    public void Continue()
    {
        mainPlayer.allLineUnit[currentPlayer]._HP = currentFightPlayer._HP;
        Debug.Log("MainPlayer : " + mainPlayer);
        Debug.Log("currentFightPlayer : " + currentFightPlayer);

        if(currentPlayer > 0)
        {
            Debug.Log("Destroy");

            for (int i = 0; i < currentPlayer; i++)
            {
                mainPlayer.allLineUnit[0].DestroyMainAvatar();
            }
        }

        Destroy(currentFightPlayer.gameObject);
        state = GameState.Play;
        gameObject.SetActive(false);

        FindObjectOfType<Sound_Manager>().Play_SelectSound("map");
    }

    IEnumerator StartFight(PlayerController currentPly, EnemyController currentEnem, bool isPlayerWin = true)
    {
        yield return new WaitForSeconds(.5f);

        bool win = false;

        while(currentPly._HP > 0 && currentEnem._HP > 0)
        {
            float damage = 0;

            if (isPlayerWin)
            {
                // PlayerAttack
                Debug.Log("PlayerAttack!");

                currentPly.anim.SetTrigger("attack");

                damage = currentPly._ATK - currentEnem._DEF;

                if (currentPly.unitData._unitType == currentEnem.unitData._unitType)
                {
                    damage = (2 * currentPly._ATK) - currentEnem._DEF;
                }
                else
                {
                    damage = currentPly._ATK - currentEnem._DEF;
                }

                yield return new WaitForSeconds(.25f);
                // Damage Calculate
                EffectHandle.current.PlaySelectEffect("hit", currentEnem.transform.position);

                if (damage <= 0) damage = 1;
                currentFightEnemy.DecreaseHP(damage);

                if (currentPly.unitData._unitType == currentEnem.unitData._unitType)
                {
                    SpawnDamageText(enemyPredestal.transform.position, damage, true);
                }
                else
                {
                    SpawnDamageText(enemyPredestal.transform.position, damage);
                }

                enemyHealth.fillAmount = currentFightEnemy._HP / currentFightEnemy.unitData._HP;
                enemyHealthText.text = currentFightEnemy._HP + "/" + currentFightEnemy.unitData._HP;

                if (currentEnem._HP <= 0)
                {
                    EffectHandle.current.PlaySelectEffect("death", currentEnem.transform.position);
                    currentFightEnemy._HP = 0;
                    enemyHealthText.text = currentFightEnemy._HP + "/" + currentFightEnemy.unitData._HP;
                    win = true;
                    break;
                }
                yield return new WaitForSeconds(.25f);
            }        


            // Enemy Attack
            Debug.Log("EnemyAttack!");
            currentEnem.anim.SetTrigger("attack");
            damage = currentEnem._ATK - currentPly._DEF;

            yield return new WaitForSeconds(.25f);
            // Damage Calculate
            EffectHandle.current.PlaySelectEffect("hit", currentPly.transform.position);
            if (damage <= 0) damage = 1;
            currentFightPlayer.DecreaseHP(damage);
            SpawnDamageText(playerPredestal.transform.position, damage);

            playerHealth.fillAmount = currentFightPlayer._HP / currentFightPlayer.unitData._HP;
            playerHealthText.text = currentFightPlayer._HP + "/" + currentFightPlayer.unitData._HP;

            if (currentPly._HP <= 0)
            {
                EffectHandle.current.PlaySelectEffect("death", currentPly.transform.position);
                currentFightPlayer._HP = 0;
                playerHealthText.text = currentFightPlayer._HP + "/" + currentFightPlayer.unitData._HP;
                win = false;
                break;
            }

            isPlayerWin = true;
            yield return new WaitForSeconds(.25f);
        }

        if (win)
        {
            currentEnemy++;
            controller.IncreaseScore(10);
            if (currentEnemy < enemyCount)
            {
                Destroy(currentFightEnemy.gameObject);
                SpawnEnemy(mainEnemy, currentEnemy);
                yield return null;
                StartCoroutine(StartFight(currentFightPlayer, currentFightEnemy));
                Debug.Log("EnemyDie!");
            }
            else
            {
                Destroy(currentFightEnemy.gameObject);
                for (int i = mainEnemy.allLineUnit.Count - 1; i>0 ; i--)
                {
                    Destroy(mainEnemy.allLineUnit[i].gameObject);
                    mainEnemy.allLineUnit.RemoveAt(i);
                }
                Destroy(mainEnemy.gameObject);

                WinCanvas.SetActive(true);
                Debug.Log("YouWin");
                isActive = false;
            }
        }
        else
        {
            currentPlayer++;
            if(currentPlayer < playerCount)
            {
                Destroy(currentFightPlayer.gameObject);
                SpawnPlayer(mainPlayer, currentPlayer);
                yield return null;
                StartCoroutine(StartFight(currentFightPlayer, currentFightEnemy, false));
                Debug.Log("PlayerDie!");
            }
            else
            {
                Destroy(currentFightPlayer.gameObject);
                Debug.Log("YouLose");
                yield return null;
                state = GameState.End;
                LoseCanvas.SetActive(true);
                endScoreText.text = "Your Score : " + controller.score;
                isActive = false;
            }
        }
    }

    void SpawnDamageText(Vector3 pos, float value, bool crit = false)
    {
        GameObject dmgtxt = Instantiate(damageText, pos, Quaternion.identity, transform);
        if (crit)
        {
            dmgtxt.GetComponentInChildren<Text>().text = "Critical Hit!! - " + value;
        }
        else
            dmgtxt.GetComponentInChildren<Text>().text = "Hit! - " + value;
        Destroy(dmgtxt, 1f);
    }
}
