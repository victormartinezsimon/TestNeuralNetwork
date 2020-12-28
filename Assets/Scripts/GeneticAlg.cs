using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlg : MonoBehaviour
{
    [Header("Elementos no configurables")]
    [SerializeField]
    private GameObject _carToCopy;
    [SerializeField]
    private Transform _startPosition;
    [SerializeField]
    private List<int> _sizeNeuralNetwork;
    [SerializeField]
    private Color _colorRunning;
    [SerializeField]
    private Color _colorStopped;
    [SerializeField]
    private Color _notSelected;
    [SerializeField]
    private int _marginBest = 4;
    
    [SerializeField]
    private Text _textoInformation;
    [SerializeField]
    private string _nameBest = "./best.txt";

    //Elementos configurables
    private int _totalCars;
    public float _randomMutation = 0.1f;
    private int _maxTimeout = 5;
    private float _randomRepeatBest = 0.5f;

    private List<GameObject> _cars;
    private List<Chromosome> _listCromosomes;
    private int _currentBestCar = -1;
    private Mutex _mutex = new Mutex();
    private bool _writeBestCarToText = false;
    private float _timeStartTraining;

    private struct car_info{
        public int id;
        public List<float> _neuralNetwork;
        public float _score;
        public long _time;
        public float _scoreAcum;
        public void SetScoreAcum(float v) { _scoreAcum = v; }
    };
    private List<car_info> _infoTraining;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);//init random
        LoadConfiguration();
        CreateCars();
        ResetCars();
        StartTraining();
    }

    private void Update()
    {
        if(Time.time > _timeStartTraining + _maxTimeout || Input.GetKeyDown(KeyCode.E))
        {
            for(int car = 0; car < _totalCars; ++car)
            {
                _listCromosomes[car].FinishTraining(true);
            }
        }
        WriteInfo();
    }

    private void LoadConfiguration()
    {
        ConfigurationManager cm = ConfigurationManager.GetInstance();
        _totalCars = cm.TotalCars;
        _randomMutation = cm.RandomMutation;
        _maxTimeout = cm.Timeout;
        _randomRepeatBest = cm.RandomRepeatBest;
    }


    /// <summary>
    /// Update the variables so the best car will be saved when the current training end
    /// </summary>
    public void SaveBestCar()
    {
        _writeBestCarToText = true;
    }

    /// <summary>
    /// Write in the screen the information of the current best car
    /// </summary>
    private void WriteInfo()
    {
        float bestDistance = -1;
        int bestIdx = 0;

        for(int car = 0; car < _totalCars; ++car)
        {
            if (_listCromosomes[car]._totalDistance > bestDistance)
            {
                bestIdx = car;
                bestDistance = _listCromosomes[car]._totalDistance;
            }
        }
        if(bestIdx != _currentBestCar)
        {
            _currentBestCar = bestIdx;
        }

        _textoInformation.text = "best car[" + bestIdx + "] => " + bestDistance + " timeout: "+ (int)(Time.time - _timeStartTraining);
    }

    /// <summary>
    /// Create all the cars, add the component, and the necessary information
    /// </summary>
    private void CreateCars()
    {
        _cars = new List<GameObject>();
        _listCromosomes = new List<Chromosome>();
        string file = Application.dataPath + _nameBest;
        for (int i = 0; i < _totalCars; ++i)
        {
            GameObject go =  Instantiate<GameObject>(_carToCopy);
            go.transform.name = "car:" + i;
            go.transform.position = _startPosition.position;
            go.transform.rotation = _startPosition.rotation;
            go.transform.parent = this.transform;
            _listCromosomes.Add(go.GetComponent<Chromosome>());
            _listCromosomes[i].Init(_sizeNeuralNetwork);
            if (System.IO.File.Exists(file) && (i % 5 == 0))
            {
                _listCromosomes[i].GetNeuralNetwork().ReadFromFile(file);
            }
            _cars.Add(go);
        }
        _infoTraining = new List<car_info>();
    }
    /// <summary>
    /// Reset the cars to the initial position and rotation
    /// </summary>
    private void ResetCars()
    {
        for (int i = 0; i < _totalCars; ++i)
        {
            _cars[i].transform.position = _startPosition.position;
            _cars[i].transform.rotation = _startPosition.rotation;
        }
    }

    /// <summary>
    /// Starts the training
    /// </summary>
    private void StartTraining()
    {
        _infoTraining.Clear();
        for (int i = 0; i < _totalCars; ++i)
        {
            _cars[i].transform.position = _startPosition.position;
            _cars[i].transform.rotation = _startPosition.rotation;
            _cars[i].GetComponent<Renderer>().material.color = _colorRunning;
            _listCromosomes[i].RunAgent(i);
        }
        _timeStartTraining = Time.time;
        _writeBestCarToText = false;
    }

    /// <summary>
    /// Function called by any element when the training ended(normally a hit)
    /// </summary>
    /// <param name="score"></param>
    /// <param name="time"></param>
    /// <param name="id"></param>
    public void TaskEnded(long time, int id, float distance)
    {
        car_info it = new car_info();
        it.id = id;
        it._score = distance;
        it._time = time;

        it._neuralNetwork = new List<float>(_listCromosomes[id].GetNeuralNetwork().ToList());
        _cars[id].GetComponent<Renderer>().material.color = _colorStopped;

        _mutex.WaitOne();
            _infoTraining.Add(it);
        _mutex.ReleaseMutex();

        if(_infoTraining.Count == _totalCars)
        {
            GeneticAlgorithm();

            if (_writeBestCarToText)
            {
                _listCromosomes[0].GetNeuralNetwork().WriteToFile(Application.dataPath +  _nameBest);
            }

            StartCoroutine(RestartTraining());
        }
    }

    /// <summary>
    /// Genetic algorithm
    /// </summary>
    private void GeneticAlgorithm()
    {
        SortBestCars();
        List<float> best = _infoTraining[0]._neuralNetwork;
        MergeCars();
        MutateCars();
        if(Random.Range(0.0f, 1.0f) < _randomRepeatBest)
        {
            MergeCars(best, best, 0);//repeat best
        }
    }

    /// <summary>
    /// Coroutine to restart the training
    /// </summary>
    /// <returns></returns>
    private IEnumerator RestartTraining()
    {
        yield return new WaitForSeconds(2);
        StartTraining();
    }

    /// <summary>
    /// Sort the best cars and delete the bad cars
    /// </summary>
    private void SortBestCars()
    {
        _infoTraining.Sort((ci1, ci2) =>
        {
           if(ci1._score.CompareTo(ci2._score) == 0)
           {
               return ci1._time.CompareTo(ci2._time);
           }
           return ci1._score.CompareTo(ci2._score);
        }
        );

        _infoTraining.Reverse();

        float bestScore = _infoTraining[0]._score;

        int totalCarsToAnalyze = 0;

        for(int i = 0; i < _totalCars; ++i)
        {
            if(_infoTraining[i]._score < bestScore - _marginBest)
            {
                break;
            }
            ++totalCarsToAnalyze;
        }

        for(int i = totalCarsToAnalyze; i < _totalCars; ++i )
        {
            _cars[_infoTraining[i].id].GetComponent<Renderer>().material.color = _notSelected;
        }

        _infoTraining.RemoveRange(totalCarsToAnalyze, _totalCars - totalCarsToAnalyze);

        float acum = 0;
        for(int i =0; i < totalCarsToAnalyze; ++i)
        {
            acum += _infoTraining[i]._score;
            _infoTraining[i].SetScoreAcum(acum);
        }
    }

    /// <summary>
    /// Function that will create all the sons
    /// </summary>
    private void MergeCars()
    {
        for(int carIdx = 0; carIdx < _totalCars; ++carIdx)
        {
            GetCarsToMerge(out int firstCar, out int secondCar);

            MergeCars(_infoTraining[firstCar]._neuralNetwork, _infoTraining[secondCar]._neuralNetwork, carIdx);
        }
    }

    /// <summary>
    /// return two cars to merge using the roulette algorithm
    /// </summary>
    /// <param name="car1"></param>
    /// <param name="car2"></param>
    private void GetCarsToMerge(out int car1, out int car2)
    {
        float maxRandom = _infoTraining[_infoTraining.Count - 1]._scoreAcum;
        car1 = 0;
        car2 = 0;

        for (int soluition = 0; soluition < 2; ++soluition)
        {
            float random = UnityEngine.Random.Range(0, maxRandom);
            float acum = 0;
            
            for (int car = 0; car < _infoTraining.Count; ++car)
            {
                acum += _infoTraining[car]._score;
                if(acum <= random)
                {
                    if(soluition == 0)
                    {
                        car1 = car;
                    }
                    else
                    {
                        car2 = car;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Merge two cars into a new car
    /// </summary>
    /// <param name="nn_first"></param>
    /// <param name="nn_second"></param>
    /// <param name="carIdx"></param>
    private void MergeCars(List<float> nn_first, List<float> nn_second, int carIdx)
    {
        NeuralNetwork nn_toWrite = _listCromosomes[carIdx].GetNeuralNetwork();

        int indexList = 0;

        for (int layer = 1; layer < _sizeNeuralNetwork.Count; ++layer)
        {
            for (int neuronIdx = 0; neuronIdx < _sizeNeuralNetwork[layer]; ++neuronIdx)
            {
                //bias
                {
                    float value = nn_first[indexList];
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5)
                    {
                        value = nn_second[indexList];
                    }

                    nn_toWrite.SetBiasValue(layer, neuronIdx, value);
                    ++indexList;
                }

                for (int neuronIdxPrevious = 0; neuronIdxPrevious < _sizeNeuralNetwork[layer - 1]; ++neuronIdxPrevious)
                {
                    float value = nn_first[indexList];
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5)
                    {
                        value = nn_first[indexList];
                    }

                    nn_toWrite.SetWeight(layer, neuronIdx, neuronIdxPrevious, value);
                    ++indexList;
                }

            }
        }
    }

    /// <summary>
    /// Mutate some cars if necesary
    /// </summary>
    private void MutateCars()
    {
        for (int carIdx = 0; carIdx < _totalCars; ++carIdx)
        {
            NeuralNetwork nn = _listCromosomes[carIdx].GetNeuralNetwork();
            for (int layer = 1; layer < _sizeNeuralNetwork.Count; ++layer)
            {
                for (int neuronIdx = 0; neuronIdx < _sizeNeuralNetwork[layer]; ++neuronIdx)
                {
                    //bias
                    {
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < _randomMutation)
                        {
                            nn.SetBiasValue(layer, neuronIdx, UnityEngine.Random.Range(-0.5f, 0.5f));
                        }
                    }

                    for (int neuronIdxPrevious = 0; neuronIdxPrevious < _sizeNeuralNetwork[layer - 1]; ++neuronIdxPrevious)
                    {
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < _randomMutation)
                        {
                            nn.SetWeight(layer, neuronIdx, neuronIdxPrevious, UnityEngine.Random.Range(-0.5f, 0.5f));
                        }
                    }
                    
                }
            }
        }
    }
}
