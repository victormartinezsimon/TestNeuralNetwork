using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeuralNetwork
{
    private List<int> _layersSize;
    private List<List<float>> _neuron_value;
    private List<List<float>> _biases;
    private List<List<List<float>>> _weights;
    private List<int> _activations;

    public NeuralNetwork(List<int> layersSize)
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);//init random
        this._layersSize = layersSize;
        InitNeurons();
        InitBiases();
        InitWeights();
    }
    /// <summary>
    /// Initialize all neurons with default value of 0
    /// </summary>
    private void InitNeurons()
    {
        _neuron_value = new List<List<float>>();
        for(int layerIdx = 0; layerIdx < _layersSize.Count; ++layerIdx)
        {
            List<float> tmp = new List<float>();

            for(int neuronIdx = 0; neuronIdx < _layersSize[layerIdx]; ++neuronIdx)
            {
                tmp.Add(0);
            }
            _neuron_value.Add(tmp);
         }
    }

    /// <summary>
    /// Init the value of the biases, some random value that is used in the activation function
    /// </summary>
    private void InitBiases()
    {
        _biases = new List<List<float>>();
        for (int layerIdx = 0; layerIdx < _layersSize.Count; ++layerIdx)
        {
            List<float> tmp = new List<float>();

            for (int biaseIdx = 0; biaseIdx < _layersSize[layerIdx]; ++biaseIdx)
            {
                tmp.Add(UnityEngine.Random.Range(-0.5f, 0.5f));
            }
            _biases.Add(tmp);
        }
    }

    /// <summary>
    /// Initialize the weights(connections) between each node from one layer to all the nodes in the next layer
    /// </summary>
    private void InitWeights()
    {
        _weights = new List<List<List<float>>>();

        List<List<float>> emptyList = new List<List<float>>();
        _weights.Add(emptyList);//there is no weight from the input

        int totalLayers = _layersSize.Count;
        for(int layerIdx = 1; layerIdx< totalLayers; ++layerIdx)
        {
            int totalNeuronsInThisLayer = _layersSize[layerIdx];
            int totalNeuronsInPreviousLayer = _layersSize [layerIdx - 1];

            List<List<float>> weightLayer = new List<List<float>>();

            for(int neuronIdx = 0; neuronIdx < totalNeuronsInThisLayer; ++neuronIdx)
            {
                List<float> weightNeuron = new List<float>();

                for(int previousNeuronIdx = 0; previousNeuronIdx < totalNeuronsInPreviousLayer; ++previousNeuronIdx)
                {
                    weightNeuron.Add(UnityEngine.Random.Range(-0.5f, 0.5f));
                }
                weightLayer.Add(weightNeuron);
            }
            _weights.Add(weightLayer);
        }
    }

    public List<float> GeValue(List<float> input)
    {
        List<float> result = new List<float>();

        //set the current value of the initial neurons to the input
        for(int neuronIdx = 0; neuronIdx < input.Count; ++neuronIdx)
        {
            _neuron_value[0][neuronIdx] = input[neuronIdx];
        }

        //calculate the value of the next neurons
        for(int layerIdx = 1; layerIdx < _layersSize.Count; ++layerIdx)
        {
            int totalNeuronsInThisLayer = _layersSize[layerIdx];
            int totalNeuronsInPreviousLayer = _layersSize[layerIdx - 1];

            for(int neuronIdx = 0; neuronIdx < totalNeuronsInThisLayer; ++neuronIdx)
            {
                float value = 0;
                for (int previousNeuronIdx = 0; previousNeuronIdx < totalNeuronsInPreviousLayer; ++previousNeuronIdx)
                {
                    value += _neuron_value[layerIdx - 1][previousNeuronIdx] * _weights[layerIdx][neuronIdx][previousNeuronIdx];
                }
                _neuron_value[layerIdx][neuronIdx] = ActivateFunction(value + _biases[layerIdx][neuronIdx]);
            }
        }

        //generate the result
        {
            int lastLayerIdx = _layersSize.Count - 1;
            for (int neuronIdx = 0; neuronIdx < _layersSize[lastLayerIdx]; ++neuronIdx)
            {
                result.Add(_neuron_value[lastLayerIdx][neuronIdx]);
            }
        }
        return result;
    }

    /// <summary>
    /// function that calculate the new value of the node
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private float ActivateFunction(float v)
    {
        return (float)System.Math.Tanh(v);
    }

    /// <summary>
    /// Save the current neural network in a file
    /// </summary>
    /// <param name="filePath"></param>
    public void WriteToFile(string filePath)
    {
        StreamWriter sw = new StreamWriter(filePath, false);

        for(int layerIdx = 1; layerIdx < _layersSize.Count; ++layerIdx)
        {
            int totalNeuronsInThisLayer = _layersSize[layerIdx];
            int totalNeuronsInPreviousLayer = _layersSize[layerIdx - 1];

            for (int neuronIdx = 0; neuronIdx < totalNeuronsInThisLayer; ++neuronIdx)
            {
                for (int previousNeuronIdx = 0; previousNeuronIdx < totalNeuronsInPreviousLayer; ++previousNeuronIdx)
                {
                    float value = _weights[layerIdx][neuronIdx][previousNeuronIdx];
                    sw.WriteLine(value);
                }
            }
        }
        sw.Flush();

        for (int layerIdx = 0; layerIdx < _layersSize.Count; ++layerIdx)
        {

            for (int biaseIdx = 0; biaseIdx < _layersSize[layerIdx]; ++biaseIdx)
            {
                float value = _biases[layerIdx][biaseIdx];
                sw.WriteLine(value);
            }
        }
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// read a previous saved neural network
    /// </summary>
    /// <param name="filePath"></param>
    public void ReadFromFile(string filePath)
    {
        StreamReader sr = new StreamReader(filePath);
        for (int layerIdx = 1; layerIdx < _layersSize.Count; ++layerIdx)
        {
            int totalNeuronsInThisLayer = _layersSize[layerIdx];
            int totalNeuronsInPreviousLayer = _layersSize[layerIdx - 1];

            for (int neuronIdx = 0; neuronIdx < totalNeuronsInThisLayer; ++neuronIdx)
            {
                for (int previousNeuronIdx = 0; previousNeuronIdx < totalNeuronsInPreviousLayer; ++previousNeuronIdx)
                {
                    float value = float.Parse(sr.ReadLine());
                    _weights[layerIdx][neuronIdx][previousNeuronIdx] = value;
                }
            }
        }

        for (int layerIdx = 0; layerIdx < _layersSize.Count; ++layerIdx)
        {

            for (int biaseIdx = 0; biaseIdx < _layersSize[layerIdx]; ++biaseIdx)
            {
                _biases[layerIdx][biaseIdx] = float.Parse(sr.ReadLine());
            }
        }

        sr.Close();
    }


}
