using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D rb;
    Camera cam;

    [SerializeField] float minSpeed = 0.2f;
    [SerializeField] float lifeTimeAfterSlow = 1.5f;

    float slowTimer;
    bool isLaunched;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    public void OnLaunched()
    {
        isLaunched = true;
    }

    public void ResetState()
    {
        isLaunched = false;
        slowTimer = 0f;
    }

    void Update()
    {
        if (!isLaunched) return; 

        CheckSpeed();
        CheckIfOutOfScreen();
    }

    void CheckSpeed()
    {
        if (rb.linearVelocity.magnitude < minSpeed)
        {
            slowTimer += Time.deltaTime;

            if (slowTimer >= lifeTimeAfterSlow)
                ReturnToPool();
        }
        else
        {
            slowTimer = 0f;
        }
    }

    void CheckIfOutOfScreen()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        bool isOutside =
            viewPos.x < 0 || viewPos.x > 1 ||
            viewPos.y < 0 || viewPos.y > 1;

        if (isOutside)
            ReturnToPool();
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}