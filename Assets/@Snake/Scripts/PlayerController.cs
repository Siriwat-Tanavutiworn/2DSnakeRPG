using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : StateLookUp
{
    GameController controller;

    [Header("Unit")]
    public MainUnitType type;
    public List<PlayerController> allLineUnit = new List<PlayerController>();
    [SerializeField] PlayerController lastedUnit;
    [SerializeField] PlayerController mainPlayer;
    [SerializeField] SpriteRenderer health;

    [Header("Attack")]
    public Animator anim;
    bool isChangePlayer = false;

    [Header ("Movement")]
    [SerializeField] Vector3 startPoint;
    [SerializeField] Vector3 endPoint;
    [SerializeField] float _startMoveSpeed = 4;
    [SerializeField] float _moveSpeed = 0;
    float moveSpeedPerUnit = .5f;
    public GameObject body;
    [SerializeField] Vector3 dir;

    public bool isPlayer = false;

    [Header("Status")]
    public UnitData unitData;
    public float _HP;
    public float _ATK;
    public float _DEF;

    private void Awake()
    {
        InitStat();
        controller = FindObjectOfType<GameController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        startPoint = transform.position;
        anim = GetComponent<Animator>();

        //GetColor();

        gameStates[GameState.Start] = () =>
        {
            StartGame();
            HealthCheck();
        };

        gameStates[GameState.Play] = () =>
        {
            if (controller.isPause) return;

            HealthCheck();
            switch (type)
            {
                case MainUnitType.MainPlayer:
                    GetLastPlayer();
                    ChangeMainPlayer();
                    Movement();
                    break;
                case MainUnitType.Follower:
                    GetMainPlayer();
                    Movement();
                    break;
                case MainUnitType.FollowerPicking:
                    break;
                default:
                    break;
            }

        };

        gameStates[GameState.Fight] = () =>
        {

        };

        gameStates[GameState.End] = () =>
        {

        };
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

    void StartGame()
    {
        if (controller.isPause) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            dir = Vector2.right;
            state = GameState.Play;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            dir = Vector2.left;
            state = GameState.Play;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            dir = Vector2.up;
            state = GameState.Play;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            dir = Vector2.down;
            state = GameState.Play;
        }

        endPoint = transform.position + dir;
    }

    void Movement()
    {
        if (isChangePlayer) return;

        _moveSpeed = _startMoveSpeed + (moveSpeedPerUnit * (allLineUnit.Count - 1));

        RaycastHit2D hitTop = Physics2D.Raycast(transform.position, Vector2.up, 1, 1 << 8);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, 1, 1 << 8);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 1, 1 << 8);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 1, 1 << 8);

        switch (type)
        {
            case MainUnitType.MainPlayer:
                if (Input.GetKeyDown(KeyCode.D) && startPoint - endPoint != Vector3.right && hitRight.collider == null)
                {
                    dir = Vector2.right;
                }
                else if (Input.GetKeyDown(KeyCode.A) && startPoint - endPoint != Vector3.left && hitLeft.collider == null)
                {
                    dir = Vector2.left;
                }
                else if (Input.GetKeyDown(KeyCode.W) && startPoint - endPoint != Vector3.up && hitTop.collider == null)
                {
                    dir = Vector2.up;
                }
                else if (Input.GetKeyDown(KeyCode.S) && startPoint - endPoint != Vector3.down && hitDown.collider == null)
                {
                    dir = Vector2.down;
                }

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

                CameraControl();
                WallCheck();
                break;
            case MainUnitType.Follower:
                transform.position = Vector3.MoveTowards(transform.position, endPoint, _moveSpeed * Time.deltaTime);
                BodyControl();
                break;
            case MainUnitType.FollowerPicking:
                break;
            default:
                break;
        }
        BodyControl();
    }

    void CameraControl()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        Camera.main.transform.position = new Vector3(Mathf.Clamp(Camera.main.transform.position.x, -4.5f, 4.5f), Mathf.Clamp(Camera.main.transform.position.y, -3f, 3f), Camera.main.transform.position.z);
    }

    void BodyControl()
    {        
        if(startPoint - endPoint == Vector3.up)
        {
            body.transform.localScale = Vector3.one;
            anim.Play("PMoveDown");
        }
        else if(startPoint - endPoint == Vector3.down)
        {
            body.transform.localScale = Vector3.one;
            anim.Play("PMoveUp");
        }
        else if (startPoint - endPoint == Vector3.left)
        {
            body.transform.localScale = Vector3.one;
            anim.Play("PMoveX");
        }
        else if (startPoint - endPoint == Vector3.right)
        {
            body.transform.localScale = new Vector3(-1, 1, 1);
            anim.Play("PMoveX");
        }
    }

    void ChangeMainPlayer()
    {
        if (allLineUnit.Count < 2) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isChangePlayer = true;

            Vector2 lastPlayerPos = allLineUnit[allLineUnit.Count - 1].transform.position;
            Vector2 lastPlayerStartPos = allLineUnit[allLineUnit.Count - 1].startPoint;
            Vector2 lastPlayerEndPos = allLineUnit[allLineUnit.Count - 1].endPoint;
            Vector3 pDir = dir;

            for (int i = allLineUnit.Count - 1; i >= 0; i--)
            {
                if (i != 0)
                {
                    allLineUnit[i].startPoint = allLineUnit[i - 1].startPoint;
                    allLineUnit[i].endPoint = allLineUnit[i - 1].endPoint;
                    allLineUnit[i].transform.position = allLineUnit[i - 1].transform.position;
                    allLineUnit[i].dir = allLineUnit[i - 1].dir;
                }
                else
                {
                    startPoint = lastPlayerStartPos;
                    endPoint = lastPlayerEndPos;

                    transform.position = lastPlayerPos;
                }
            }

            allLineUnit.RemoveAt(0);
            allLineUnit.Add(this);
            //dir = Vector2.zero;

            allLineUnit[0].dir = pDir;

            for (int i = 0; i < allLineUnit.Count - 1; i++)
            {
                allLineUnit[i].allLineUnit = allLineUnit;
            }
            type = MainUnitType.Follower;

            StartCoroutine(Delayed());
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            isChangePlayer = true;
            Vector3 pos = transform.position;

            Vector2 startPos = startPoint;
            Vector2 endPos = endPoint;

            Vector3 pDir = dir;

            for (int i = 0; i < allLineUnit.Count; i++)
            {
                if (i == 0)
                {
                    startPoint = allLineUnit[1].startPoint;
                    endPoint = allLineUnit[1].endPoint;
                    transform.position = allLineUnit[1].transform.position;
                    dir = allLineUnit[i].dir;
                }
                else if (i != allLineUnit.Count - 1)
                {
                    allLineUnit[i].startPoint = allLineUnit[i + 1].startPoint;
                    allLineUnit[i].endPoint = allLineUnit[i + 1].endPoint;
                    allLineUnit[i].transform.position = allLineUnit[i + 1].transform.position;
                    allLineUnit[i].dir = allLineUnit[i + 1].dir;
                }
                else
                {
                    allLineUnit[i].startPoint = startPos;
                    allLineUnit[i].endPoint = endPos;
                    allLineUnit[i].transform.position = pos;
                    allLineUnit[i].dir = pDir;
                }
            }

            allLineUnit.Insert(0, allLineUnit[allLineUnit.Count - 1]);
            allLineUnit[1] = this;
            allLineUnit.RemoveAt(allLineUnit.Count - 1);

            for (int i = 0; i < allLineUnit.Count - 1; i++)
            {
                allLineUnit[i].allLineUnit = allLineUnit;
            }

            type = MainUnitType.Follower;
            StartCoroutine(Delayed());
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            DestroyMainAvatar();
        }
    }

    IEnumerator Delayed(bool first = false)
    {
        yield return null;
        allLineUnit[0].type = MainUnitType.MainPlayer;
        isChangePlayer = false;
        if (first) Destroy(gameObject);

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

            DestroyAvatarByWall();
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
                dir = Vector2.left;
            }

            DestroyAvatarByWall();
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

            DestroyAvatarByWall();
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
                dir = Vector2.down;
            }

            DestroyAvatarByWall();
        }
    }

    void HealthCheck()
    {
        if (_HP >= unitData._HP || state != GameState.Play)
        {
            health.transform.parent.gameObject.SetActive(false);
        }
        else if(_HP < unitData._HP && state == GameState.Play)
        {
            health.transform.parent.gameObject.SetActive(true);
            health.size = new Vector2((_HP / unitData._HP) * 3.33f, health.size.y);
        }

       

        if (_HP <= 0)
        {
            int index = 0;
            switch (type)
            {
                case MainUnitType.MainPlayer:
                    break;
                case MainUnitType.Follower:
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

            DestroyAvatarByWall();
        }
    }

    public void DecreaseHP(float value)
    {
        _HP -= value;
    }

    public void DestroyAvatarByWall()
    {
        EffectHandle.current.PlaySelectEffect("death", transform.position);

        if(allLineUnit.Count < 2)
        {
            Destroy(gameObject);
            state = GameState.End;
        }
        else
        {
            //Destroy(allLineUnit[allLineUnit.Count - 1].gameObject);
            //allLineUnit.RemoveAt(allLineUnit.Count - 1);
            DestroyMainAvatar();
        }

    }

    public void DestroyMainAvatar()
    {
        for (int i = allLineUnit.Count - 1; i > 0; i--)
        {
            allLineUnit[i].startPoint = allLineUnit[i - 1].startPoint;
            allLineUnit[i].endPoint = allLineUnit[i - 1].endPoint; ;
            allLineUnit[i].transform.position = allLineUnit[i - 1].transform.position;
            allLineUnit[i].dir = allLineUnit[i - 1].dir;
        }

        allLineUnit.RemoveAt(0);

        for (int i = 0; i < allLineUnit.Count; i++)
        {
            allLineUnit[i].allLineUnit = allLineUnit;
        }
        type = MainUnitType.Follower;

        StartCoroutine(Delayed(true));
        Destroy(gameObject, .1f);
    }

    void GetLastPlayer()
    {
        lastedUnit = allLineUnit[allLineUnit.Count - 1];
    }

    void GetMainPlayer()
    {
        mainPlayer = allLineUnit[0];
    }

    #region OldCode
    //void GetColor()
    //{
    //    switch (unitData._unitType)
    //    {
    //        case UnitData.UnitType.Red:
    //            body.GetComponent<SpriteRenderer>().color = new Color32(219, 138, 138, 255);
    //            break;
    //        case UnitData.UnitType.Green:
    //            body.GetComponent<SpriteRenderer>().color = new Color32(114, 251, 113, 255);
    //            break;
    //        case UnitData.UnitType.Blue:
    //            body.GetComponent<SpriteRenderer>().color = new Color32(137, 144, 219, 255);
    //            break;
    //        default:
    //            break;
    //    }
    //}


    //public void IncreaseSpeed(float value)
    //{
    //    foreach (var item in allLineUnit)
    //    {
    //        item._moveSpeed += value;
    //    }
    //}
    #endregion

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PlayerController>())
        {
            var newFollower = col.GetComponent<PlayerController>();
            if (newFollower.type == MainUnitType.FollowerPicking)
            {
                Debug.Log(col.name);

                if (type == MainUnitType.MainPlayer)
                {
                    allLineUnit.Add(newFollower);
                    for (int i = 1; i < allLineUnit.Count; i++)
                    {
                        allLineUnit[i].allLineUnit = allLineUnit[0].allLineUnit;
                    }

                    newFollower.transform.position = allLineUnit[allLineUnit.Count - 2].transform.position - allLineUnit[allLineUnit.Count - 2].dir;
                    newFollower.type = MainUnitType.Follower;

                    newFollower.startPoint = allLineUnit[allLineUnit.Count - 2].startPoint;
                    newFollower.endPoint = allLineUnit[allLineUnit.Count - 2].endPoint;
                    newFollower._moveSpeed = _moveSpeed;

                    EffectHandle.current.PlaySelectEffect("hitFollower", col.transform.position);
                }
                
            }
            else if (newFollower.type == MainUnitType.Follower)
            {
                if (type == MainUnitType.MainPlayer)
                {
                    int index = 0;
                    for (int i = 0; i < allLineUnit.Count; i++)
                    {
                        if (newFollower == allLineUnit[i])
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index < 2) return;
                    else
                    {
                        for (int i = allLineUnit.Count - 1; i >= index; i--)
                        {
                            Destroy(allLineUnit[i].gameObject);
                            allLineUnit.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

}
