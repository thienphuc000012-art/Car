using UnityEngine;

public class NitroPickup : MonoBehaviour
{
    public float nitroAmount = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            CarController car = other.GetComponent<CarController>();
            if (car != null)
            {
                car.AddNitroEnergy(nitroAmount);
                Destroy(gameObject);
            }
        }
    }

}
