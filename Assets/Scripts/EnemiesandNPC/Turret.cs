using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject player;
    public GameObject ballPrefab;

    public float secondsBetweenShots = 2f;
    public float maxDistance = 10f;
    public float ballSpeed = 10f;
    public float damage = 10f;

    private float shootTimer = 0f;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= maxDistance)
        {
            shootTimer += Time.deltaTime;

            if (shootTimer >= secondsBetweenShots)
            {
                shootBall();
                shootTimer = 0f;
            }
        }
        else
        {
            shootTimer = 0f;
        }
    }

    void shootBall()
    {
        GameObject ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);

        Vector3 direction = player.transform.position - transform.position;
        direction.Normalize();

        TurretBall turretBall = ball.GetComponent<TurretBall>();
        turretBall.direction = direction;
        turretBall.speed = ballSpeed;
        turretBall.maxDistance = maxDistance;
        turretBall.damage = damage;
    }
}
