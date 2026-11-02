using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MenuStateTrigger : MonoBehaviour
{
    public enum Result
    {
        Win,
        Lose
    }

    [SerializeField] private Result result;
    
    [Header("Win Requirements")]
    public int requiredPieces = 4;

    private bool triggered;
    private Collider gate;

    // popup variables
    private int missingPiecesCount = 0;
    private float textAlpha = 0f;
    private GUIStyle style;

    public Result TriggerResult
    {
        get => result;
        set => result = value;
    }

    private void Start()
    {
        // style for the missing pieces popup matching portal color hex 19F266
        style = new GUIStyle()
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color32(25, 242, 102, 255) } 
        };

        gate = GetComponents<Collider>()[1];
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        if (result == Result.Win)
        {
            // check if player has enough pieces to enter
            PlayerPuzzlePiece playerPieces = other.GetComponentInParent<PlayerPuzzlePiece>();
            int currentPieces = playerPieces != null ? playerPieces.GetPieceCount() : 0;
            
            if (currentPieces < requiredPieces)
            {
                // calculate missing and show warning
                missingPiecesCount = requiredPieces - currentPieces;
                StopAllCoroutines();
                StartCoroutine(FadeSequence());
                return;
            } else
            {
                gate.isTrigger = true; // allow the gate to be passed thru if enough pieces
            }
        }

        PenguinMenuController menu = FindFirstObjectByType<PenguinMenuController>();
        if (menu == null)
        {
            return;
        }

        triggered = true;
        if (result == Result.Win)
        {
            menu.ShowWin();
        }
        else
        {
            menu.ShowLose();
        }
    }

    private IEnumerator FadeSequence()
    {
        // fade in quickly
        for (float t = 0f; t < 0.25f; t += Time.deltaTime)
        {
            textAlpha = Mathf.Lerp(0f, 1f, t / 0.25f);
            yield return null;
        }
        textAlpha = 1f;

        // hold on screen
        yield return new WaitForSeconds(2f);

        // fade out slowly
        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            textAlpha = Mathf.Lerp(1f, 0f, t / 1f);
            yield return null;
        }
        textAlpha = 0f;
    }

    private void OnGUI()
    {
        if (textAlpha > 0f)
        {
            Color original = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, textAlpha);
            
            // display just below the checkpoint popup area
            Rect position = new Rect(0, 110f, Screen.width, 50f);
            GUI.Label(position, $"MISSING {missingPiecesCount} PUZZLE PIECE(S) TO ENTER", style);
            
            GUI.color = original;
        }
    }
}