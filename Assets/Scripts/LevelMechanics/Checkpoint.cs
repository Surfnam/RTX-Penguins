using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Renderer visualRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;
    
    private bool isActivated = false;

    private void Start()
    {
        // grab the renderer on this pad if not manually assigned
        if (visualRenderer == null)
        {
            visualRenderer = GetComponent<Renderer>();
        }

        // set starting color
        if (visualRenderer != null && inactiveMaterial != null)
        {
            visualRenderer.sharedMaterial = inactiveMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag("Player"))
        {
            isActivated = true;
            
            // set new respawn point to the center of this pad
            other.GetComponent<PlayerController>()?.SetCheckpoint(transform.position);

            // change visual to show it is active
            if (visualRenderer != null && activeMaterial != null)
            {
                visualRenderer.sharedMaterial = activeMaterial;
            }

            // trigger the screen popup
            CheckpointPopup popup = FindFirstObjectByType<CheckpointPopup>();
            if (popup != null)
            {
                popup.ShowPopup();
            }
        }
    }
}