
using UnityEngine;

public class PlayerSpriteAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Animator animator;

    [Header("Animator States")]
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string walkState = "Walk";

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.0001f;

    private Vector3 previousPosition;
    private bool isWalking;

    private void Awake()
    {
        if (playerRoot == null)
            playerRoot = transform.parent;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (playerRoot != null)
            previousPosition = playerRoot.position;

        isWalking = false;

        if (animator != null)
            animator.Play(idleState, 0, 0f);
    }

    private void LateUpdate()
    {
        if (playerRoot == null || animator == null)
            return;

        Vector3 currentPosition = playerRoot.position;

        float distanceSquared =
            (currentPosition - previousPosition).sqrMagnitude;

        bool moving =
            distanceSquared >
            movementThreshold * movementThreshold;

        if (moving != isWalking)
        {
            isWalking = moving;

            animator.CrossFade(
                isWalking ? walkState : idleState,
                0.05f,
                0
            );
        }

        previousPosition = currentPosition;
    }
}
