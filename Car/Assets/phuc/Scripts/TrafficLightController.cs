using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [Header("UI Countdown")]
    public TextMeshProUGUI countdownText;

    [Header("Traffic Lights")]
    public GameObject redLight;
    public GameObject yellowLight;
    public GameObject greenLight;

    [Header("Cars - Kéo tất cả xe đã đặt sẵn vào đây")]
    public List<CarController> cars;

    void Start()
    {
        SetAllCarsLocked(true);          
        StartCoroutine(StartRaceCountdown());
    }

    IEnumerator StartRaceCountdown()
    {

        redLight.SetActive(true);
        yellowLight.SetActive(false);
        greenLight.SetActive(false);
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);


        redLight.SetActive(false);
        yellowLight.SetActive(true);
        greenLight.SetActive(false);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        redLight.SetActive(false);
        yellowLight.SetActive(false);
        greenLight.SetActive(true);
        countdownText.text = "1"; 
        yield return new WaitForSeconds(1f);

        ReleaseCarsSmoothly();

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        RaceManager.Instance.StartRace();
    }


    private void SetAllCarsLocked(bool locked)
    {
        foreach (var car in cars)
        {
            if (car == null) continue;

            car.raceStarted = !locked;

            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

      
            foreach (var wc in car.wheelColliders)
            {
                if (wc != null)
                {
                    wc.motorTorque = 0;
                    wc.brakeTorque = locked ? car.brakeForce * 8f : 0;
                }
            }
        }
    }


    private void ReleaseCarsSmoothly()
    {
        foreach (var car in cars)
        {
            if (car == null) continue;

            car.raceStarted = true;

            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

        
            foreach (var wc in car.wheelColliders)
            {
                if (wc != null)
                {
                    wc.brakeTorque = 0;
                    wc.motorTorque = 0; 
                }
            }
        }
    }
}