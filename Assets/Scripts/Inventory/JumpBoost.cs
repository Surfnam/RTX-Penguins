using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class JumpBoost : MonoBehaviour
{
    public float jumpMultiplier = 1.5f;
    public float duration = 10f;
    public GameObject boostEffect;

    public float TimeLeft { get; private set; }

    private float normalJumpHeight;

    private void OnEnable()
    {
        GetComponent<PlayerController>().OnPlayerDeath += Cancel;
    }

    private void OnDisable()
    {
        GetComponent<PlayerController>().OnPlayerDeath -= Cancel;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // Drinking during a boost only restarts the timer, boosts don't stack.
    public void Activate()
    {
        if (TimeLeft <= 0f)
        {
            PlayerController player = GetComponent<PlayerController>();
            normalJumpHeight = player.baseJumpHeight;
            player.baseJumpHeight = normalJumpHeight * jumpMultiplier;
            if (boostEffect != null) boostEffect.SetActive(true);
        }
        TimeLeft = duration;
    }

    public void Tick(float deltaTime)
    {
        if (TimeLeft <= 0f)
        {
            return;
        }

        TimeLeft -= deltaTime;
        if (TimeLeft <= 0f)
        {
            End();
        }
    }

    public void Cancel()
    {
        if (TimeLeft > 0f)
        {
            End();
        }
    }

    // shortcut: restores the height saved at Activate, so a permanent jump bonus gained mid-boost is lost, change to a separate multiplier when side puzzle bonuses exist
    private void End()
    {
        TimeLeft = 0f;
        GetComponent<PlayerController>().baseJumpHeight = normalJumpHeight;
        if (boostEffect != null) boostEffect.SetActive(false);
    }
}
