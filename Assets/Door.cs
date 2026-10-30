using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public int requiredPieces = 4;

    private bool isUnlocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isUnlocked) {
            PlayerPuzzlePiece inventory = other.GetComponent<PlayerPuzzlePiece>();
            if (inventory != null) {
                if (inventory.GetPieceCount() >= requiredPieces) {
                    UnlockDoor(inventory);
                } else {
                    Debug.Log($"Locked! You only have {inventory.GetPieceCount()}/{requiredPieces} pieces. You cannot pass.");
                }
            }
        }
    }

    private void UnlockDoor(PlayerPuzzlePiece inventory)
    {
        isUnlocked = true;
        inventory.ClearInventory();
        Debug.Log("Door unlocked successfully! Access granted.");
        gameObject.SetActive(false);
    }

}
