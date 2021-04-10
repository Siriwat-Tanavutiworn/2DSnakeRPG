using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : StateLookUp
{
    GameController controller;

    public EnemyType type;
    public List<EnemyController> allLineUnit = new List<EnemyController>();
    public GameObject body;

    [Header("Attack")]
    public Animator anim;
    bool isAttack = false;

    [Header("Movement")]
    public Vector3 startPoint;
    public Vector3 endPoint;
    public float _moveSpeed;

    [Header("Direction")]
    public Vector3 dir;
    public float maxChangeDirTimer;
    [SerializeField] float currentChangeDirTimer;

    [Header("Status")]
    public UnitData unitData;
    public float _HP;
    public float _ATK;
    public float _DEF;
    public float _ATKSPD;

    private void Awake()
    {
        InitStat();
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        controller = FindObjectOfType<GameController>();

        //InitDirection();
        currentChangeDirTimer = maxChangeDirTimer;
        //GetColor();

        gameStates[GameState.Start] = () =>
        {

        };

        gameStates[GameState.Play] = () =>
        {
            if (controller.isPause) return;

            Movement();
            HealthCheck();
        };

        gameStates[GameState.Fight] = () =>
        {

        };

        gameStates[GameState.End] = () =>
        {

        };
    }

    public void InitDirection()
    {
        int rand = Random.Range(0, 4);
        switch (rand)
        {
            case 0:
                dir = Vector2.up;
                break;
            case 1:
                dir = Vector2.down;
                break;
            case 2:
                dir = Vector2.left;
                break;
            case 3:
                dir = Vector2.right;
                break;
            default:
                break;
        }

        startPoint = transform.position;
        endPoint = transform.position + dir;
    }

    void InitStat()
    {
        _HP = unitData._HP;
        _ATK = unitData._ATK;
        _DEF = unitData._DEF;
    }

    // Update is called once per frame
    void Update()
    {
        gameStates[state]();
    }

    void Movement()
    {
        switch (type)
        {
            case EnemyType.MainEnemy:

                if (Vector2.Distance(transform.position, endPoint) == 0)
                {
                    for (int i = allLineUnit.Count - 1; i > 0; i--)
                    {
                        allLineUnit[i].startPoint = allLineUnit[i - 1].startPoint;
                        allLineUnit[i].endPoint = allLineUnit[i - 1].endPoint;

                        allLineUnit[i].dir = allLineUnit[i].endPoint - allLineUnit[i].startPoint;

                    }

                    startPoint = endPoint;
                    endPoint = transform.position + dir;
                }

                transform.position = Vector3.MoveTowards(transform.position, endPoint, _moveSpeed * Time.deltaTime);

                EnemyCheck();
                WallCheck();
                AutoChangeDirection();
                break;
            case EnemyType.EnemyFollower:
                transform.position = Vector3.MoveTowards(transform.position, endPoint, _moveSpeed * Time.deltaTime);
                break;
            default:
                break;
        }

        BodyControl();
    }

    void WallCheck()
    {
        RaycastHit2D hitTop = Physics2D.Raycast(transform.position, Vector2.up, 1, 1 << 8);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 1, 1 << 8);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 1, 1 << 8);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 1, 1 << 8);

        if (dir == Vector3.up && hitTop.collider != null)
        {
            if (hitLeft.collider != null)
            {
                dir = Vector2.right;
            }
            else if (hitRight.collider != null)
            {
                dir = Vector2.left;
            }
            else
            {
                dir = Vector2.left;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.down && hitDown.collider != null)
        {
            if (hitLeft.collider != null)
            {
                dir = Vector2.right;
            }
            else if (hitRight.collider != null)
            {
                dir = Vector2.left;
            }
            else
            {
                dir = Vector2.right;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.left && hitLeft.collider != null)
        {
            if (hitTop.collider != null)
            {
                dir = Vector2.down;
            }
            else if (hitDown.collider != null)
            {
                dir = Vector2.up;
            }
            else
            {
                dir = Vector2.down;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.right && hitRight.collider != null)
        {
            if (hitTop.collider != null)
            {
                dir = Vector2.down;
            }
            else if (hitDown.collider != null)
            {
                dir = Vector2.up;
            }
            else
            {
                dir = Vector2.up;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
    }

    void EnemyCheck()
    {
        RaycastHit2D hitTop = Physics2D.Raycast(transform.position + Vector3.up , Vector2.up, 1, 1 << 9);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position + Vector3.down, Vector2.down, 1, 1 << 9);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + Vector3.left, Vector2.left, 1, 1 << 9);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + Vector3.right, Vector2.right, 1, 1 << 9);
        
        RaycastHit2D hitTL = Physics2D.Raycast(transform.position + Vector3.up + Vector3.left , Vector3.up + Vector3.left, 1, 1 << 9);
        RaycastHit2D hitTR = Physics2D.Raycast(transform.position + Vector3.up + Vector3.right, Vector3.up + Vector3.right, 1, 1 << 9);
        RaycastHit2D hitDL = Physics2D.Raycast(transform.position + Vector3.left + Vector3.down, Vector3.left + Vector3.down, 1, 1 << 9);
        RaycastHit2D hitDR = Physics2D.Raycast(transform.position + Vector3.right + Vector3.down, Vector3.right + Vector3.down, 1, 1 << 9);

        if (dir == Vector3.up && hitTop.collider != null)
        {
            Debug.Log("@Top");
            if (hitLeft.collider != null || hitTL.collider != null)
            {
                dir = Vector2.right;
            }
            else if (hitRight.collider != null || hitTR.collider != null)
            {
                dir = Vector2.left;
            }
            else
            {
                dir = Vector2.left;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.down && hitDown.collider != null)
        {

            Debug.Log("@Down");

            if (hitLeft.collider != null || hitDL.collider != null)
            {
                dir = Vector2.right;
            }
            else if (hitRight.collider != null || hitDR.collider != null)
            {
                dir = Vector2.left;
            }
            else
            {
                dir = Vector2.right;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.left && hitLeft.collider != null)
        {
            Debug.Log("@Left");
            if (hitTop.collider != null || hitTL.collider != null)
            {
                dir = Vector2.down;
            }
            else if (hitDown.collider != null || hitDL.collider != null)
            {
                dir = Vector2.up;
            }
            else
            {
                dir = Vector2.down;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }
        else if (dir == Vector3.right && hitRight.collider != null)
        {
            Debug.Log("@Right");
            if (hitTop.collider != null || hitTR.collider != null)
            {
                dir = Vector2.down;
            }
            else if (hitDown.collider != null || hitDR.collider != null)
            {
                dir = Vector2.up;
            }
            else
            {
                dir = Vector2.up;
            }
            currentChangeDirTimer = maxChangeDirTimer;
        }

    }

    void BodyControl()
    {

        if (startPoint - endPoint == Vector3.up)
        {
            anim.Play("EMoveDown");
        }
        else if (startPoint - endPoint == Vector3.down)
        {
            body.transform.localScale = Vector3.one;
            anim.Play("EMoveUp");
        }
        else if (startPoint - endPoint == Vector3.left)
        {
            body.transform.localScale = Vector3.one;
            anim.Play("EMoveX");
        }
        else if (startPoint - endPoint == Vector3.right)
        {
            body.transform.localScale = new Vector3(-1, 1, 1);
            anim.Play("EMoveX");
        }
    }

    void AutoChangeDirection()
    {
        currentChangeDirTimer -= Time.deltaTime;

        if(currentChangeDirTimer <= 0)
        {
            RaycastHit2D hitTop = Physics2D.Raycast(transform.position, Vector2.up, 1, 1 << 8);
            RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 1, 1 << 8);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 1, 1 << 8);
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 1, 1 << 8);

            int rand = Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    if(dir == Vector3.down)
                    {
                        if(hitLeft.collider != null)
                        {
                            dir = Vector2.right;
                        }
                        else if(hitRight.collider != null)
                        {
                            dir = Vector2.left;
                        }
                    }
                    else
                    {
                        dir = Vector2.up;
                    }
                    break;
                case 1:
                    if (dir == Vector3.up)
                    {
                        if (hitLeft.collider != null)
                        {
                            dir = Vector2.right;
                        }
                        else if(hitRight.collider != null)
                        {
                            dir = Vector2.left;
                        }
                    }
                    else
                    {
                        dir = Vector2.down;
                    }
                    break;
                case 2:

                    if (dir == Vector3.right)
                    {
                        if(hitTop.collider != null)
                        {
                            dir = Vector2.down;
                        }else if(hitDown.collider != null)
                        {
                            dir = Vector2.up;
                        }
                    }
                    else
                    {
                        dir = Vector2.left;
                    }
                    break;
                case 3:
                    if (dir == Vector3.left)
                    {
                        if (hitTop.collider != null)
                        {
                            dir = Vector2.down;
                        }
                        else if (hitDown.collider != null)
                        {
                            dir = Vector2.up;
                        }
                    }
                    else
                    {
                        dir = Vector2.right;
                    }
                    break;
                default:
                    break;
            }

            currentChangeDirTimer = maxChangeDirTimer;
        }
    }

    void GetColor()
    {
        switch (unitData._unitType)
        {
            case UnitData.UnitType.Red:
                body.GetComponent<SpriteRenderer>().color = new Color32(219, 138, 138, 255);
                break;
            case UnitData.UnitType.Green:
                body.GetComponent<SpriteRenderer>().color = new Color32(114, 251, 113, 255);
                break;
            case UnitData.UnitType.Blue:
                body.GetComponent<SpriteRenderer>().color = new Color32(137, 144, 219, 255);
                break;
            default:
                break;
        }
    }

    void HealthCheck()
    {
        if(_HP <= 0)
        {
            int index = 0;
            switch (type)
            {
                case EnemyType.MainEnemy:                    
                    break;
                case EnemyType.EnemyFollower:
                    for (int i = 0; i < allLineUnit.Count; i++)
                    {
                        if (allLineUnit[i] == this)
                        {
                            index = i;
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }

            for (int i = allLineUnit.Count - 1; i >= index; i--)
            {
                Destroy(allLineUnit[i].gameObject);
                allLineUnit.RemoveAt(i);
            }

        }
    }

    public void DecreaseHP(float value)
    {
        _HP -= value;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PlayerController>())
        {
            var player = col.GetComponent<PlayerController>();
            if (player.type == MainUnitType.MainPlayer)
            {
                Debug.Log("@Hit");
                controller.CallFighting(player, allLineUnit[0]);
            }
        }
    }

}
