using UnityEngine;
using System.Collections;

public class CheckpointPopup : MonoBehaviour
{
    private float textAlpha = 0f;
    private GUIStyle style;

    private void Start()
    {
        // setup style with the requested active color hex 016A00
        style = new GUIStyle()
        {
            alignment = TextAnchor.UpperCenter,
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color32(1, 106, 0, 255) } 
        };
    }

    public void ShowPopup()
    {
        StopAllCoroutines();
        StartCoroutine(FadeSequence());
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
            
            // gui color multiplies the alpha of the style color
            GUI.color = new Color(1f, 1f, 1f, textAlpha);
            
            // draw near top of screen just below the floor hud
            Rect position = new Rect(0, 70f, Screen.width, 50f);
            GUI.Label(position, "CHECKPOINT REACHED", style);
            
            GUI.color = original;
        }
    }
}