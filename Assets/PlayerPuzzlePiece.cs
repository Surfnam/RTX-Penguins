using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPuzzlePiece : MonoBehaviour
{
    private List<PuzzlePiece> collectedPieces = new List<PuzzlePiece>();

    public void AddPiece(PuzzlePiece piece) {
        collectedPieces.Add(piece);
    }

    public List<PuzzlePiece> getCollectedPieces()
    {
        return collectedPieces;
    }

    public void SaveCollectedPieces()
    {
        foreach (var piece in collectedPieces)
        {
            piece.SetSavedAtCheckpoint();
        }
    }

    public int GetPieceCount() {
        return collectedPieces.Count;
    }

    // safely empty the list on death without destroying the physical items
    public void ClearPieces() 
    {
        collectedPieces.RemoveAll(piece => !piece.savedAtCheckpoint);
    }

    public void ClearInventory() {
        foreach (var piece in collectedPieces) {
            Destroy(piece.gameObject);
        }
        collectedPieces.Clear();
    }
}