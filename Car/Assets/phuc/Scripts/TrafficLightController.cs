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

    [Header("Cars")]
    public List<CarController> cars;

    void Start()
    {
        StartCoroutine(StartRaceCountdown());
    }

    IEnumerator StartRaceCountdown()
    {
        // Khóa xe trong lúc đếm ngược
        SetAllCarsLocked(true);

        // Đèn đỏ
        redLight.SetActive(true);
        yellowLight.SetActive(false);
        greenLight.SetActive(false);

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        // Đèn vàng + GO!
        redLight.SetActive(false);
        yellowLight.SetActive(true);
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        // Đèn xanh
        yellowLight.SetActive(false);
        greenLight.SetActive(true);

        // Mở khóa xe
        SetAllCarsLocked(false);

        // Ẩn countdown
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void SetAllCarsLocked(bool locked)
    {
        foreach (var car in cars)
        {
            if (car == null) continue;

            Rigidbody rb = car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (locked)
                {
                    // Trong lúc countdown: chặn xe chạy
                    car.raceStarted = false;

                    // Giữ xe bằng phanh
                    foreach (var wc in car.wheelColliders)
                    {
                        wc.brakeTorque = car.brakeForce * 10f;
                        wc.motorTorque = 0;
                    }
                }
                else
                {
                    // Countdown xong: mở khóa xe
                    rb.linearVelocity = Vector3.zero;        // reset vận tốc
                    rb.angularVelocity = Vector3.zero; // reset xoay

                    foreach (var wc in car.wheelColliders)
                    {
                        wc.brakeTorque = 0; // bỏ phanh
                    }

                    car.raceStarted = true;
                }
            }
        }
    }


}
