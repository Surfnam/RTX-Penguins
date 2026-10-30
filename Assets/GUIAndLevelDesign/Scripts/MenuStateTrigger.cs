using UnityEngine;

/// <summary>
/// A small handoff point between level geometry and the menu UI.
/// It deliberately does not implement player health, checkpoints, or inventory loss.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MenuStateTrigger : MonoBehaviour
{
    public enum Result
    {
        Win,
        Lose
    }

    [SerializeField] private Result result;
    private bool triggered;

    public Result TriggerResult
    {
        get => result;
        set => result = value;
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
}
