using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] Player _playerPrefab;
    [SerializeField] Enemy _enemyPrefab;
    public Player player;
    public float gameSpeed = 1.0f;
    [SerializeField] List<Enemy> _enemies = new();
    Vector3 _playerStartPos = new();
    Vector3 _playerStartScale = new();
    List<Vector3> _enemyStartPoses = new();
    List<Vector3> _enemyStartScales = new();
    List<float> _maxDistances = new();
    int _enemyCount = 0;
    public Text GoalText { get; set; } = null;
    public Transform _goalTransform = null;
    public float maxTime = 12.0f;
    public bool IsManualMode { get; set; } = false;
    public float LastDistanceFromGoal { get; set; } = 100.0f;
    List<float> _distancesFromGoal = new();
    public float Score
    {
        get
        {
            float minDistance = _distancesFromGoal.Count >= 1 ? _distancesFromGoal.Min() : 100.0f;

            float score = 1f / minDistance * 100;

            if (player.IsGoal)
            {
                score += 100000;
            }

            return score;
        }
    }

    void Awake()
    {
        SetUpStage();
    }

    public void StartStage()
    {
        player.CanMove = true;

        foreach (Enemy enemy in _enemies)
        {
            enemy.CanMove = true;
        }
    }

    void Update()
    {
        if (player != null && player.IsGoal && GoalText != null)
        {
            GoalText.gameObject.SetActive(true);
        }
    }

    public void SetUpStage()
    {
        _enemyCount = _enemies.Count;

        foreach (Enemy enemy in _enemies)
        {
            enemy.Player = player;

            enemy.gameSpeed = gameSpeed;
        }

        player.gameSpeed = gameSpeed;
        player.IsManualMode = IsManualMode;
        player.MaxTime = maxTime;
        player.onEndMoveAction = SetDistanceFromGoal;

        _playerStartPos = player.transform.localPosition;
        _playerStartScale = player.transform.localScale;

        _enemyStartPoses = _enemies.Map(enemy => enemy.transform.localPosition);
        _enemyStartScales = _enemies.Map(enemy => enemy.transform.localScale);
        _maxDistances = _enemies.Map(enemy => enemy.maxDistance);
    }

    public async Task StageTask()
    {
        while (!player.IsDead && !player.IsGoal)
        {
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
        }
    }

    public void SetDistanceFromGoal()
    {
        _distancesFromGoal.Add(Vector3.Distance(_goalTransform.transform.position, player.transform.position));
    }
}
