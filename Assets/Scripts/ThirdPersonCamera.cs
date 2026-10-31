using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        transform.LookAt(player.position + Vector3.up * 1.2f);
    }
}
