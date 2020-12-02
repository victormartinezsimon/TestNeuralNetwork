using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chromosome : MonoBehaviour
{
    public float _distanceRay = 2.0f;
    public float rotationMultiplier = 10;
    public float speed = 10;

    GeneticAlg _geneticAlg;
    NeuralNetwork _nw;
    private float timeStart;
    int _checkPointsPassed = 0;
    private bool running = false;
    private int ID;


    private int lastCheckpoint = -1;
    private int _totalCheckpoints = 10;

    public void Init(List<int> neuralNetwork, int totalChecpoints)
    {
        _geneticAlg = GetComponentInParent<GeneticAlg>();
        _nw = new NeuralNetwork(neuralNetwork);
        this._totalCheckpoints = totalChecpoints;
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            CalculateNextPosition();
        }
    }

    private void CalculateNextPosition()
    {
        RaycastHit hit;
        List<float> distances = new List<float>();

        for (int i = 0; i < 5; i++)//draws five debug rays as inputs
        {
            Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;
            if (Physics.Raycast(transform.position, newVector, out hit, _distanceRay))
            {
                distances.Add(hit.distance);
            }
            else
            {
                distances.Add(_distanceRay);
            }
            Debug.DrawRay(transform.position, newVector, Color.red);

        }

        List<float> result = _nw.GetValue(distances);
        
        //GetComponent<Rigidbody>().MoveRotation(transform.Rotate(0, result[0] * rotationMultiplier, 0, Space.World);//controls the cars movement
        GetComponent<Rigidbody>().position +=(this.transform.right * result[1] * speed);
        //transform.position += ;//controls the cars turning
    }

    void OnCollisionEnter(Collision collision)
    {
        //finish the work
        FinishTraining();
    }

    private void FinishTraining()
    {
        running = false;
        float diffTime = System.DateTime.Now.Ticks - timeStart;
        _geneticAlg.TaskEnded(_checkPointsPassed, diffTime, ID);
    }
    private void OnTriggerExit(Collider other)
    {
        Checkpoint ch = other.GetComponent<Checkpoint>();
        if(ch != null)
        {
            if(ch.ID == lastCheckpoint +1)
            {
                ++_checkPointsPassed;
                lastCheckpoint = ch.ID;

                if(_checkPointsPassed >= _totalCheckpoints)
                {
                    FinishTraining();
                }
            }
        }
    }

    public void RunAgent(int id)
    {
        this.ID = id;
        timeStart = System.DateTime.Now.Ticks;
        running = true;
        lastCheckpoint = -1;
    }

    public NeuralNetwork GetNeuralNetwork()
    {
        return _nw;
    }
}