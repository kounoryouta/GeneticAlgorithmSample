using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class GManager : MonoBehaviour
{
    [SerializeField] Transform _stageManagerParent;
    [SerializeField] StageManager _stageManagerPrefab;
    [SerializeField] int _individualCount = 10;
    [SerializeField] float _simulationSpeed = 10;
    [SerializeField] int _framePerAction = 6;
    [SerializeField] float _maxTime = 12.0f;
    [SerializeField] int _maxGeneration = 100;
    [SerializeField] bool _isManualMode = false;
    [SerializeField] Text _goalText = null;
    [SerializeField] float _mutePossiblility = 0.2f;
    GeneticAlgorithm_2DAction _geneticAlgorithm = new();
    int _actionIDCount = 0;
    List<StageManager> _stageManagers = new();
    [SerializeField] Button _startGameButton;
    [SerializeField] Text _generationText;

    async void Start()
    {
        UnityEngine.Random.InitState(RandomUtility.getRamdom());

        if (_generationText != null)
        {
            _generationText.gameObject.SetActive(false);
        }

        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(true);
            _startGameButton.onClick.RemoveAllListeners();
            _startGameButton.onClick.AddListener(StartGame);
            _startGameButton.transform.GetChild(0).GetComponent<Text>().text = "Start!";
        }
    }

    public async void StartGame()
    {
        for (int i = 0; i < _stageManagers.Count; i++)
        {
            if (_stageManagers[i] != null)
            {
                DestroyImmediate(_stageManagers[i].gameObject);
            }
        }

        if (_isManualMode)
        {
            if (_startGameButton != null)
            {
                _startGameButton.gameObject.SetActive(true);
                _startGameButton.onClick.RemoveAllListeners();
                _startGameButton.onClick.AddListener(StartGame);
                _startGameButton.transform.GetChild(0).GetComponent<Text>().text = "Restart!";
            }

            StageManager stageManager = Instantiate(_stageManagerPrefab, _stageManagerParent);
            stageManager.gameSpeed = 1;
            stageManager.IsManualMode = true;
            stageManager.GoalText = _goalText;
            _stageManagers.Add(stageManager);
            stageManager.SetUpStage();
            stageManager.StartStage();
        }

        else
        {

            if (_startGameButton != null)
            {
                _startGameButton.gameObject.SetActive(false);
            }

            _actionIDCount = (int)(_maxTime / Time.fixedDeltaTime / _framePerAction);

            _geneticAlgorithm.ActionTask = PlayStage;
            _geneticAlgorithm.ActionTaskForChildren = PlayStageForChildren;
            _geneticAlgorithm.ActionIDCount = _actionIDCount;
            _geneticAlgorithm.IndividualsCount = _individualCount;
            _geneticAlgorithm.FitnessFunc = FitnessFunc;
            _geneticAlgorithm.MaxGeneration = _maxGeneration;
            _geneticAlgorithm.MutePossiblility = _mutePossiblility;

            await Search();

            if (_generationText != null)
            {
                _generationText.gameObject.SetActive(true);
                _generationText.text = $"{_geneticAlgorithm.GenerationIndex}世代目, 最大適合度 : {_geneticAlgorithm.MaxFitness}, 平均適合度 : {_geneticAlgorithm.AverageFitness}";
            }

            Individual<int> bestIndividual = _geneticAlgorithm.BestIndividual;

            if (bestIndividual != null)
            {
                int[] answerActionIDs = bestIndividual.Chromosome.CloneArray();

                if (_startGameButton != null)
                {
                    _startGameButton.gameObject.SetActive(true);
                    _startGameButton.onClick.RemoveAllListeners();
                    _startGameButton.onClick.AddListener(() => PlayAnswer(answerActionIDs));
                    _startGameButton.transform.GetChild(0).GetComponent<Text>().text = "結果を見る";
                }
            }
        }
    }

    void PlayAnswer(int[] answerActionIDs)
    {
        if (answerActionIDs != null)
        {
            for (int i = 0; i < _stageManagers.Count; i++)
            {
                if (_stageManagers[i] != null)
                {
                    DestroyImmediate(_stageManagers[i].gameObject);
                }
            }

            StageManager stageManager = Instantiate(_stageManagerPrefab, _stageManagerParent);
            stageManager.gameSpeed = _simulationSpeed;
            stageManager.IsManualMode = false;
            stageManager.SetUpStage();
            stageManager.player.ActionIDs = answerActionIDs;

            stageManager.StartStage();
        }
    }

    async Task Search()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        await _geneticAlgorithm.Search();
    }

    async Task PlayStage()
    {
        if (_generationText != null)
        {
            _generationText.gameObject.SetActive(true);
            _generationText.text = $"{_geneticAlgorithm.GenerationIndex}世代目, 最大適合度 : {_geneticAlgorithm.MaxFitness}, 平均適合度 : {_geneticAlgorithm.AverageFitness}";
        }

        for (int i = 0; i < _stageManagers.Count; i++)
        {
            if (_stageManagers[i] != null)
            {
                DestroyImmediate(_stageManagers[i].gameObject);
            }
        }

        _stageManagers = new();

        for (int i = 0; i < _individualCount; i++)
        {
            StageManager stageManager = Instantiate(_stageManagerPrefab, _stageManagerParent);
            stageManager.gameSpeed = _simulationSpeed;
            stageManager.IsManualMode = false;
            stageManager.SetUpStage();
            stageManager.player.ActionIDs = _geneticAlgorithm.Individuals[i].Chromosome.CloneArray();

            _stageManagers.Add(stageManager);
        }

        foreach (StageManager stageManager in _stageManagers)
        {
            stageManager.StartStage();
        }

        List<Task> stageTasks = _stageManagers.Map(stageManager => stageManager.StageTask());

        await Task.WhenAll(stageTasks);
    }

    async Task PlayStageForChildren(Individual<int>[] children)
    {
        for (int i = 0; i < _stageManagers.Count; i++)
        {
            DestroyImmediate(_stageManagers[i].gameObject);
        }

        _stageManagers = new();

        for (int i = 0; i < children.Length; i++)
        {
            StageManager stageManager = Instantiate(_stageManagerPrefab, _stageManagerParent);
            stageManager.gameSpeed = _simulationSpeed;
            stageManager.IsManualMode = false;
            stageManager.SetUpStage();
            stageManager.player.ActionIDs = children[i].Chromosome.CloneArray();

            _stageManagers.Add(stageManager);
        }

        foreach (StageManager stageManager in _stageManagers)
        {
            stageManager.StartStage();
        }

        List<Task> stageTasks = _stageManagers.Map(stageManager => stageManager.StageTask());

        await Task.WhenAll(stageTasks);

        for (int i = 0; i < _stageManagers.Count; i++)
        {
            if (_stageManagers[i] != null)
            {
                DestroyImmediate(_stageManagers[i].gameObject);
            }
        }
    }

    float FitnessFunc(List<Individual<int>> individuals, Individual<int> individual)
    {
        return _stageManagers[individuals.IndexOf(individual)].Score;
    }
}
