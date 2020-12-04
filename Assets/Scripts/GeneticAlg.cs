using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlg : MonoBehaviour
{
    public GameObject _carToCopy;
    public int _totalCars;
    private List<GameObject> _cars;
    public Transform _startPosition;
    public float randomMutation = 0.1f;
    public List<int> _sizeNeuralNetwork;
    public int numCarsToMerge = 10;
    public int totalCheckPoints = 10;
    public Color _colorRun;
    public Color _colorStop;
    public Color _notSelected;
    public Color _colorBest;
    public int maxTimeout = 60;
    private float _timeStartTimeOut;
    public Text _textoInf;
    private List<Chromosome> _listCromosomes;
    int currentBestCar = -1;

    private Mutex mut = new Mutex();

    private struct car_info{
        public int id;
        public List<float> _neuralNetwork;
        public float _distance;
        public float _time;
        public float _distanceAcum;
        public void SetDistanceAcum(float v) { _distanceAcum = v; }
    };
    private List<car_info> _infoTraining;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);//init random
        CreateCars();
        ResetCars();
        StartTraining();
    }

    private void Update()
    {
        WriteInfo();
        if(Time.time > _timeStartTimeOut + maxTimeout || Input.GetKeyDown(KeyCode.S))
        {
            for(int car = 0; car < _totalCars; ++car)
            {
                _listCromosomes[car].FinishTraining(true);
            }
        }

    }

    private void WriteInfo()
    {
        float bestDistance = -1;
        int bestIdx = 0;

        for(int car = 0; car < _totalCars; ++car)
        {
            if (_listCromosomes[car]._distanceAcum > bestDistance)
            {
                bestIdx = car;
                bestDistance = _listCromosomes[car]._distanceAcum;
            }
        }
        if(bestIdx != currentBestCar)
        {
            currentBestCar = bestIdx;
            _timeStartTimeOut = Time.time;
        }
        else
        {
            if(_listCromosomes[currentBestCar].running)
            {
                _timeStartTimeOut = Time.time;
            }
        }

        _textoInf.text = "best car[" + bestIdx + "] => " + bestDistance + " timeout: "+ (int)(Time.time - _timeStartTimeOut);
    }

    private void CreateCars()
    {
        _cars = new List<GameObject>();
        _listCromosomes = new List<Chromosome>();
        for (int i = 0; i < _totalCars; ++i)
        {
            GameObject go =  Instantiate<GameObject>(_carToCopy);
            go.transform.name = "car:" + i;
            go.transform.position = _startPosition.position;
            go.transform.rotation = _startPosition.rotation;
            go.transform.parent = this.transform;
            _listCromosomes.Add(go.GetComponent<Chromosome>());
            _listCromosomes[i].Init(_sizeNeuralNetwork, totalCheckPoints);
            _cars.Add(go);
        }
        _infoTraining = new List<car_info>();
        if(_totalCars < numCarsToMerge)
        {
            numCarsToMerge = _totalCars;
        }
    }

    private void ResetCars()
    {
        for (int i = 0; i < _totalCars; ++i)
        {
            _cars[i].transform.position = _startPosition.position;
            _cars[i].transform.rotation = _startPosition.rotation;
        }
    }

    private void StartTraining()
    {
        _infoTraining.Clear();
        for (int i = 0; i < _totalCars; ++i)
        {
            _cars[i].transform.position = _startPosition.position;
            _cars[i].transform.rotation = _startPosition.rotation;
            _cars[i].GetComponent<Renderer>().material.color = _colorRun;
            _listCromosomes[i].RunAgent(i);
        }
        _timeStartTimeOut = Time.time;
    }

    public void TaskEnded(float distance, float time, int id)
    {
        car_info it = new car_info();
        it.id = id;
        it._distance = distance;
        it._neuralNetwork = new List<float>(_listCromosomes[id].GetNeuralNetwork().ToList());
        _cars[id].GetComponent<Renderer>().material.color = _colorStop;

        mut.WaitOne();
            _infoTraining.Add(it);
        mut.ReleaseMutex();

        if(_infoTraining.Count == _totalCars)
        {
            SortBestCars();
            MergeCars();
            MutateCars();
            StartCoroutine(ResetartTraining());
        }
    }

    private IEnumerator ResetartTraining()
    {
        yield return new WaitForSeconds(2);
        StartTraining();
    }

    private void SortBestCars()
    {
        _infoTraining.Sort((ci1, ci2) =>
        {
            return ci1._distance.CompareTo(ci2._distance);
        }
        );

        _infoTraining.Reverse();

        for(int i = numCarsToMerge; i < _totalCars; ++i )
        {
            _cars[_infoTraining[i].id].GetComponent<Renderer>().material.color = _notSelected;
        }

        _infoTraining.RemoveRange(numCarsToMerge, _totalCars - numCarsToMerge);

        float acum = 0;
        for(int i =0; i < numCarsToMerge; ++i)
        {
            acum += _infoTraining[i]._distance;
            _infoTraining[i].SetDistanceAcum(acum);
        }
    }

    private void MergeCars()
    {
        for(int carIdx = 0; carIdx < _totalCars; ++carIdx)
        {
            GetCarsToMerge(out int firstCar, out int secondCar);

            MergeCars(_infoTraining[firstCar]._neuralNetwork, _infoTraining[secondCar]._neuralNetwork, carIdx);
        }
    }

    private void GetCarsToMerge(out int car1, out int car2)
    {
        float maxRandom = _infoTraining[_infoTraining.Count - 1]._distanceAcum;
        car1 = 0;
        car2 = 0;

        for (int soluition = 0; soluition < 2; ++soluition)
        {
            float random = UnityEngine.Random.Range(0, maxRandom);
            float acum = 0;
            
            for (int car = 0; car < numCarsToMerge; ++car)
            {
                acum += _infoTraining[car]._distance;
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
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < randomMutation)
                        {
                            nn.SetBiasValue(layer, neuronIdx, UnityEngine.Random.Range(-0.5f, 0.5f));
                        }
                    }

                    for (int neuronIdxPrevious = 0; neuronIdxPrevious < _sizeNeuralNetwork[layer - 1]; ++neuronIdxPrevious)
                    {
                        if (UnityEngine.Random.Range(0.0f, 1.0f) < randomMutation)
                        {
                            nn.SetWeight(layer, neuronIdx, neuronIdxPrevious, UnityEngine.Random.Range(-0.5f, 0.5f));
                        }
                    }
                    
                }
            }
        }
    }
}
