using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour
{
    private static ConfigurationManager _instance;

    [Header("Elementos configurables")]
    [SerializeField]
    private int _totalCars = 100;
    [SerializeField]
    [Range(0.001f, 0.2f)]
    public float _randomMutation = 0.1f;
    [SerializeField]
    private int _maxTimeout = 5;
    [SerializeField]
    [Range(0.1f, 0.9f)]
    private float _randomRepeatBest = 0.5f;

    public int TotalCars { get { return _totalCars; } set { _totalCars = value; } }
    public float RandomMutation { get { return _randomMutation; } set { _randomMutation = value; } }
    public int Timeout { get { return _maxTimeout; } set { _maxTimeout = value; } }
    public float RandomRepeatBest { get { return _randomRepeatBest; }set { _randomRepeatBest = value; } }



    // Start is called before the first frame update
    void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        _instance = this;
    }

    public static ConfigurationManager GetInstance()
    {
        return _instance;
    }
}
