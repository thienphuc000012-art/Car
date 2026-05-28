using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RaceManager.Instance.PlayerCrossCheckpoint(checkpointIndex);
        }
    }
}
