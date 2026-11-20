using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // kill player instantly
            other.GetComponent<PlayerController>()?.Die();
        }
    }
}