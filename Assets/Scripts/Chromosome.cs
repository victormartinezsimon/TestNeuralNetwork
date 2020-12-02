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
    int _totalCheckpoints = 0;
    private bool running = false;
    private int ID;

    int count = 0;

    public void Init(List<int> neuralNetwork)
    {
        _geneticAlg = GetComponentInParent<GeneticAlg>();
        _nw = new NeuralNetwork(neuralNetwork);
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            CalculateNextPosition();
            if(count == 10)
            {
                FinishTraining();
            }
            ++count;
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
        
        transform.Rotate(0, result[0] * rotationMultiplier, 0, Space.World);//controls the cars movement
        transform.position += this.transform.right * result[1] * speed;//controls the cars turning
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
        _geneticAlg.TaskEnded(_totalCheckpoints, diffTime, ID);
    }
    private void OnTriggerExit(Collider other)
    {
        ++_totalCheckpoints;//improve, for example, check the id is increased(no backwards)
    }

    public void RunAgent(int id)
    {
        this.ID = id;
        timeStart = System.DateTime.Now.Ticks;
        running = true;
    }

    public NeuralNetwork GetNeuralNetwork()
    {
        return _nw;
    }
}