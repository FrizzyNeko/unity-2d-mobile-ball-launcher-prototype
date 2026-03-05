using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class BallHandler : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] GameObject ballPrefab;
    [SerializeField] Rigidbody2D pivot;

    [Header("Pool Settings")]
    [SerializeField] int poolSize = 15;

    [Header("Timing")]
    [SerializeField] float detachDelay = 0.15f;
    [SerializeField] float respawnDelay = 1f;

    [Header("Pull Settings")]
    [SerializeField] float maxPullDistance = 5f;

    List<Rigidbody2D> ballPool = new();

    Rigidbody2D currentBallRigidbody;
    SpringJoint2D currentBallSpringJoint;
    Finger activeFinger;

    Camera mainCamera;
    bool isDragging;

    bool _isLaunched;
    public bool isLaunched => _isLaunched;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Start()
    {
        CreatePool();
        SpawnNewBallFromPool();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        GetTouchInput();
        LimitPullDistance();
        CheckOutOfBounds();
    }

    void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab);
            ball.SetActive(false);
            ballPool.Add(ball.GetComponent<Rigidbody2D>());
        }
    }

    void SpawnNewBallFromPool()
    {
        foreach (var ball in ballPool)
        {
            if (!ball.gameObject.activeInHierarchy)
            {
                ball.transform.position = pivot.position;

                ball.linearVelocity = Vector2.zero;
                ball.angularVelocity = 0f;

                ball.bodyType = RigidbodyType2D.Kinematic;

                ball.gameObject.SetActive(true);

                currentBallRigidbody = ball;
                currentBallSpringJoint = ball.GetComponent<SpringJoint2D>();

                currentBallSpringJoint.enabled = true;
                currentBallSpringJoint.connectedBody = pivot;

                ball.GetComponent<Ball>().ResetState();

                _isLaunched = false;
                return;
            }
        }

        Debug.LogWarning("Pool'da boş top yok!");
    }

    void GetTouchInput()
    {
        if (currentBallRigidbody == null) return;

        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        if (activeTouches.Count > 0)
        {
            isDragging = true;
            Vector2 sumScreenPos = Vector2.zero;

            foreach (var touch in activeTouches)
            {
                sumScreenPos += touch.screenPosition;
            }

            Vector2 averageScreenPos = sumScreenPos / activeTouches.Count;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(averageScreenPos);
            worldPos.z = 0f;

            currentBallRigidbody.bodyType = RigidbodyType2D.Kinematic;
            currentBallRigidbody.position = worldPos;
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                LaunchBall();
            }
        }
    }

    void LimitPullDistance()
    {
        if (currentBallRigidbody == null || !isDragging) return;

        Vector2 dir = currentBallRigidbody.position - pivot.position;
        if (dir.magnitude > maxPullDistance)
        {
            dir = dir.normalized * maxPullDistance;
            currentBallRigidbody.position = pivot.position + dir;
        }
    }

    void CheckOutOfBounds()
    {
        if (currentBallRigidbody == null) return;

        Vector3 pos = currentBallRigidbody.position;
        Vector3 screenPos = mainCamera.WorldToViewportPoint(pos);

        if (screenPos.x < -0.1f || screenPos.x > 1.1f || screenPos.y < -0.1f || screenPos.y > 1.1f)
        {
            Ball ball = currentBallRigidbody.GetComponent<Ball>();
            ball.ReturnToPool();

            currentBallRigidbody = null;
            currentBallSpringJoint = null;

            _isLaunched = false;

            SpawnNewBallFromPool();
        }
    }

    void LaunchBall()
    {
        currentBallRigidbody.bodyType = RigidbodyType2D.Dynamic;

        Ball ball = currentBallRigidbody.GetComponent<Ball>();
        ball.OnLaunched();

        currentBallRigidbody = null;
        _isLaunched = true;

        StartCoroutine(DetachBallAfterDelay());
    }

    IEnumerator DetachBallAfterDelay()
    {
        yield return new WaitForSeconds(detachDelay);

        if (currentBallSpringJoint != null)
            currentBallSpringJoint.enabled = false;

        currentBallSpringJoint = null;

        yield return new WaitForSeconds(respawnDelay);

        SpawnNewBallFromPool();
    }
}