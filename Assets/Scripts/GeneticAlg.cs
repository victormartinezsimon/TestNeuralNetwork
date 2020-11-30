using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlg : MonoBehaviour
{
    public GameObject _car;
    public int _totalCars;
    private List<GameObject> _cars;
    public Transform _startPosition;

    // Start is called before the first frame update
    void Start()
    {
        CreateCars();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < _totalCars; ++i)
            {
                _cars[i].GetComponent<Chromosome>().RunAgent();
            }
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
    }


    public void TaskEnded(float totalCheckpoints, float time)
    {

    }
}
