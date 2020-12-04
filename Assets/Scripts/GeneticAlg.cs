﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
    public int maxTimeout = 60;
    private float _timeStart;

    private Mutex mut = new Mutex();

    private struct car_info{
        public int id;
        public List<float> _neuralNetwork;
        public float _score;
        public void calculateScore(int _totalch, float totalTime, int totalCheckPoints, int maxTimeOut)
        {
            _score = _totalch;

            if(_totalch > totalCheckPoints)
            {
                _score += maxTimeOut - totalTime;
            }
        }
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
        if(Time.time > _timeStart + maxTimeout)
        {
            for(int car = 0; car < _totalCars; ++car)
            {
                _cars[car].GetComponent<Chromosome>().FinishTraining(true);
            }
        }
    }

    private void CreateCars()
    {
        _cars = new List<GameObject>();
        for (int i = 0; i < _totalCars; ++i)
        {
            GameObject go =  Instantiate<GameObject>(_carToCopy);
            go.transform.name = "car:" + i;
            go.transform.position = _startPosition.position;
            go.transform.rotation = _startPosition.rotation;
            go.transform.parent = this.transform;
            go.GetComponent<Chromosome>().Init(_sizeNeuralNetwork, totalCheckPoints);
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
            _cars[i].GetComponent<Chromosome>().RunAgent(i);
        }
        _timeStart = Time.time;
    }

    public void TaskEnded(int totalCh, float time, int id)
    {
        car_info it = new car_info();
        it.id = id;
        it._neuralNetwork = new List<float>(_cars[id].GetComponent<Chromosome>().GetNeuralNetwork().ToList());
        it.calculateScore(totalCh, time, this.totalCheckPoints, this.maxTimeout);
        _cars[id].GetComponent<Renderer>().material.color = _colorStop;

        mut.WaitOne();
            _infoTraining.Add(it);
        mut.ReleaseMutex();

        if(_infoTraining.Count == _totalCars)
        {
            SortBestCars();
            MergeCars();
            MutateCars();
            StartTraining();
        }
    }

    private void SortBestCars()
    {
        _infoTraining.Sort((ci1, ci2) =>
        {
            return ci1._score.CompareTo(ci2._score);
        }
        );

        _infoTraining.Reverse();
        _infoTraining.RemoveRange(numCarsToMerge, _totalCars - numCarsToMerge);
    }

    private void MergeCars()
    {
        for(int carIdx = 0; carIdx < _totalCars; ++carIdx)
        {
            int firstCar = UnityEngine.Random.Range(0, numCarsToMerge);
            int secondCar =  UnityEngine.Random.Range(0, numCarsToMerge);

            MergeCars(_infoTraining[firstCar]._neuralNetwork, _infoTraining[secondCar]._neuralNetwork, carIdx);
        }
    }

    private void MergeCars(List<float> nn_first, List<float> nn_second, int carIdx)
    {
        NeuralNetwork nn_toWrite = _cars[carIdx].GetComponent<Chromosome>().GetNeuralNetwork();

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
            NeuralNetwork nn = _cars[carIdx].GetComponent<Chromosome>().GetNeuralNetwork();
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
