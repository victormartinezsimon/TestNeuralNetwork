using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlg : MonoBehaviour
{
    public GameObject _car;
    public int _totalCars;
    private List<GameObject> _cars;
    public Transform _startPosition;

    private struct car_info{
        public int id;
        public float totalCheckpoints;
        public float totalTime;
    };
    private List<car_info> _infoTraining;

    // Start is called before the first frame update
    void Start()
    {
        CreateCars();
        ResetCars();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartTraining();
        }
    }

    private void CreateCars()
    {
        _cars = new List<GameObject>();
        for (int i = 0; i < _totalCars; ++i)
        {
            GameObject go =  Instantiate<GameObject>(_car);
            go.transform.name = "car:" + i;
            go.transform.position = _startPosition.position;
            go.transform.rotation = _startPosition.rotation;
            go.transform.parent = this.transform;
            _cars.Add(go);
        }
        _infoTraining = new List<car_info>();
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
            _cars[i].GetComponent<Chromosome>().RunAgent(i);
        }
    }

    public void TaskEnded(float totalCheckpoints, float time, int id)
    {
        car_info it = new car_info();
        it.id = id;
        it.totalCheckpoints = totalCheckpoints;
        it.totalTime = time;

        _infoTraining.Add(it);

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
            if(ci1.totalCheckpoints > ci2.totalCheckpoints) { return 1; }
            if(ci1.totalCheckpoints < ci2.totalCheckpoints) { return -1; }
            if(ci1.totalCheckpoints == ci2.totalCheckpoints)
            {
                if(ci1.totalTime < ci2.totalTime) { return 1; }
                if(ci1.totalTime > ci2.totalTime) { return -1; }
            }
            return 0;
        }
        );
    }

    private void MergeCars()
    {

    }

    private void MutateCars()
    {

    }
}
