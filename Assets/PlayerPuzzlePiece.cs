using System.Collections.Generic;
using UnityEngine;

public class PlayerPuzzlePiece : MonoBehaviour
{
    private List<PuzzlePiece> collectedPieces = new List<PuzzlePiece>();

    public void AddPiece(PuzzlePiece piece) {
        collectedPieces.Add(piece);
    }

    public int GetPieceCount() {
        return collectedPieces.Count;
    }

    public void ClearInventory() {
        foreach (var piece in collectedPieces) {
            Destroy(piece.gameObject);
        }
        collectedPieces.Clear();
    }

}
