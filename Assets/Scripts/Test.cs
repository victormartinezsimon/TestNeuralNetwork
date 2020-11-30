using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private NeuralNetwork _neuralNetwork;
    // Start is called before the first frame update
    void Start()
    {
        List<int> sizes = new List<int>() { 5, 4, 3 };
        _neuralNetwork = new NeuralNetwork(sizes);   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            List<float> tmp = new List<float>() { 1, 2, 3, 4 };
            List<float> result = _neuralNetwork.GeValue(tmp);
            for(int i = 0; i < result.Count; ++i)
            {
                Debug.Log("i =>" + i + " = " + result[i]);
            }
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            _neuralNetwork.WriteToFile("./test.txt");
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            _neuralNetwork.ReadFromFile("./test.txt");
        }
    }
}
