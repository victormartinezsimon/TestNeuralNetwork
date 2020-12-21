using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinishConfiguration : MonoBehaviour
{
    [SerializeField]
    private Slider _totalCars;
    [SerializeField]
    private Text _totalCarsText;

    [SerializeField]
    private Slider _timeout;
    [SerializeField]
    private Text _timeoutText;

    [SerializeField]
    private Slider _percentMutation;
    [SerializeField]
    private Text _percentMutationText;

    [SerializeField]
    private Slider _percentBestCar;
    [SerializeField]
    private Text _percentBestCarText;

    private void Start()
    {
        UpdateLabelTotalCars();
        UpdateLabelTimeout();
        UpdateLabelRandomMutation();
        UpdateLabelPercentBestCar();
    }

    public void StartTraining()
    {
        ConfigurationManager cm = ConfigurationManager.GetInstance();

        cm.TotalCars = (int)_totalCars.value;
        cm.Timeout = (int)_timeout.value;
        cm.RandomMutation = _percentMutation.value;
        cm.RandomRepeatBest = _percentBestCar.value;

        SceneManager.LoadScene("Training");
    }

    public void UpdateLabelTotalCars()
    {
        _totalCarsText.text = _totalCars.value.ToString();
    }

    public void UpdateLabelTimeout()
    {
        _timeoutText.text = _timeout.value.ToString();
    }

    public void UpdateLabelRandomMutation()
    {
        _percentMutationText.text = _percentMutation.value.ToString();
    }

    public void UpdateLabelPercentBestCar()
    {
        _percentBestCarText.text = _percentBestCar.value.ToString();
    }
}
