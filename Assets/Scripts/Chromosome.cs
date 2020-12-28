using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chromosome : MonoBehaviour
{
    public float _distanceRay = 2.0f;
    public float rotationMultiplier = 10;
    public float speed = 10;
    public bool _paintRays = false;

    GeneticAlg _geneticAlg;
    NeuralNetwork _nw;
    private long _timeStart;
    public bool running = false;
    private int ID;
    public int _checkpointsPassed = 0;

    private string lastCheckpoint ="";
    private Rigidbody _rigidBody;

    public float _totalDistance;

    private bool _someCheckpointPassed = false;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (running)
        {
            CalculateNextPosition();
        }
    }

    /// <summary>
    /// Initialize the chromosome
    /// </summary>
    /// <param name="neuralNetwork"></param>
    public void Init(List<int> neuralNetwork)
    {
        _geneticAlg = GetComponentInParent<GeneticAlg>();
        _nw = new NeuralNetwork(neuralNetwork);
    }

    /// <summary>
    /// Calculathe where this element must be using the neural network
    /// </summary>
    private void CalculateNextPosition()
    {
        RaycastHit hit;
        List<float> distances = new List<float>();

        List<Color> _colors = new List<Color>() { Color.red, Color.white, Color.black, Color.green, Color.yellow };
        List<float> _angles = new List<float>() { -90, -45, 0, 45, 90 };

        for (int i = 0; i < 5; i++)//draws five debug rays as inputs
        {
            Vector3 newVector = Quaternion.AngleAxis(_angles[i], new Vector3(0,0,1)) * transform.right;
            float distance = _distanceRay;
            if (Physics.Raycast(transform.position, newVector, out hit, _distanceRay,LayerMask.GetMask("Wall")))
            {
                distances.Add(hit.distance);
                distance = hit.distance;
            }
            else
            {
                distances.Add(_distanceRay);
            }
            if(_paintRays)
            {
                Debug.DrawRay(transform.position, newVector * distance, _colors[i]);
            }
        }
        
        List<float> result = _nw.GetValue(distances);

        Vector3 rotation = _rigidBody.rotation.eulerAngles;
        rotation += result[0] * Vector3.forward;
        _rigidBody.rotation = Quaternion.Euler(rotation);

        Vector3 oldPosition = _rigidBody.position;
        if(result[1] > 0)
        {
            _rigidBody.position += (this.transform.right * result[1] * speed);
        }

        _totalDistance += (oldPosition - _rigidBody.position).magnitude;
    }

    /// <summary>
    /// If there is a collision, the training is ended
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //finish the work
        FinishTraining(false);
    }

    /// <summary>
    /// Finish the training
    /// </summary>
    /// <param name="forze"></param>
    public void FinishTraining(bool forze)
    {
        if(running)
        {
            running = false;
            long diffTime = System.DateTime.Now.Ticks - _timeStart;
            _geneticAlg.TaskEnded(diffTime, ID, _totalDistance);
        }
    }

    /// <summary>
    /// If we collide with a trigger, we check if the collider is not a past collider.
    /// If its a past collider, stop
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        Checkpoint ch = other.GetComponent<Checkpoint>();

        if(!_someCheckpointPassed)
        {
            //first time
            if(ch._endCheckPoint)
            {
                //turn back before start
                FinishTraining(false);
                return;
            }
            else
            {
                if(ch._startCheckpoint)
                {
                    ++_checkpointsPassed;
                    lastCheckpoint = ch.transform.name;
                    _someCheckpointPassed = true;
                }
                else
                {
                    //imposible, teletransport!!!
                    FinishTraining(false);
                    return;
                }
            }
        }
        else
        {
            //everything correct
            if (ch.previous.transform.name.CompareTo(lastCheckpoint) == 0)
            {
                lastCheckpoint = ch.transform.name;
                ++_checkpointsPassed;
                return;
            }
            else
            {
                //turn back
                FinishTraining(false);
                return;
            }
        }

        /*
        Checkpoint ch = other.GetComponent<Checkpoint>();
        if(!ch.previous && lastCheckpoint.Length == 0)
        {
            //first checkpoint
            lastCheckpoint = ch.transform.name;
            ++_score;
            return;
        }
      
        if(ch.previous && lastCheckpoint.Length == 0)
        {
            //turn back before start
            FinishTraining(false);
            return;
        }

        if (!ch.previous && lastCheckpoint.Length != 0)
        {
            //turn back before start or complete lap
            FinishTraining(false);
            return;
        }

        //here the ch have previous and lastcheckpoint have value
        if(ch.previous.transform.name.CompareTo(lastCheckpoint) == 0)
        {
            lastCheckpoint = ch.transform.name;
            ++_score;
            return;
        }
        else
        {
            //somtehing wrong, some chekcpoint skiped
            FinishTraining(false);
            return;
        }
        */

    }
    /// <summary>
    /// Start the training
    /// </summary>
    /// <param name="id"></param>
    public void RunAgent(int id)
    {
        this.ID = id;
        _timeStart = System.DateTime.Now.Ticks;
        running = true;
        _checkpointsPassed = 0;
        lastCheckpoint = "";
        _totalDistance = 0;
        _someCheckpointPassed = false;
    }

    /// <summary>
    /// Resturn the neural network
    /// </summary>
    /// <returns></returns>
    public NeuralNetwork GetNeuralNetwork()
    {
        return _nw;
    }
}