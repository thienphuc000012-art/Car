using UnityEngine;
using System.Collections;

public class NitroPickup : MonoBehaviour
{
    public float nitroAmount = 3f;
    public float respawnTime = 10f;   
    public float rotationSpeed = 90f; 

    private Collider col;
    private Renderer rend; 

    private void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>(); 
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponent<CarController>();
        if (car != null)
        {
            car.AddNitroEnergy(nitroAmount);

            CarAudioController audioCtrl = car.GetComponent<CarAudioController>();
            if (audioCtrl != null)
            {
                audioCtrl.PlayPickupSound();
            }

            col.enabled = false;
            if (rend != null) rend.enabled = false;
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        col.enabled = true;
        if (rend != null) rend.enabled = true;
    }
}
