using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateLookUp : MonoBehaviour
{
    public delegate void gamefunction();
    public enum GameState
    {
        Start, Play, Fight, End
    }

    public enum MainUnitType
    {
        MainPlayer, Follower, FollowerPicking
    }

    public enum EnemyType
    {
        MainEnemy, EnemyFollower
    }


    public static GameState state;
    public Dictionary<GameState, gamefunction> gameStates = new Dictionary<GameState, gamefunction>();
}
