using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractionGrabLaunch : InteractiveObject
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Scale Settings")]
    [SerializeField] private Vector3 grabbedScale = Vector3.one * 0.6f;
    [SerializeField] private float scaleSpeed = 6f;

    [Header("Launch Settings")]
    [SerializeField] private float launchForce = 12f;

    [Header("Collision Handling")]
    [Tooltip("Collider físico que se desactiva al agarrar")]
    [SerializeField] private Collider physicalCollider;

    [Header("Safety")]
    [SerializeField] private float desyncReleaseDelay = 0.25f;

    private Rigidbody objectRb;
    private Transform grabTarget;

    private Vector3 originalScale;
    private bool isGrabbed;

    private float desyncTimer;

    private void Awake()
    {
        objectRb = GetComponent<Rigidbody>();
        objectRb.interpolation = RigidbodyInterpolation.Interpolate;

        originalScale = transform.localScale;

        if (physicalCollider == null)
            physicalCollider = GetComponent<Collider>();
    }

    protected override void OnArrived() { }

    protected override void OnLeave()
    {
        // Salir del trigger = soltar sin lanzar
        if (isGrabbed)
            Release(false);
    }

    protected override void OnInteractStart()
    {
        if (currentInteractor == null)
            return;

        grabTarget = FindGrabTarget(currentInteractor);
        if (grabTarget == null)
            return;

        isGrabbed = true;
        desyncTimer = 0f;

        objectRb.useGravity = false;
        objectRb.linearVelocity = Vector3.zero;
        objectRb.angularVelocity = Vector3.zero;

        if (physicalCollider != null)
            physicalCollider.enabled = false;
    }

    protected override void OnInteract() { }

    private void Update()
    {
        if (!isGrabbed)
            return;

        // === SAFETY CHECKS ===
        if (currentInteractor == null || grabTarget == null)
        {
            desyncTimer += Time.deltaTime;

            if (desyncTimer >= desyncReleaseDelay)
            {
                Release(false);
                return;
            }
        }
        else
        {
            desyncTimer = 0f;
        }

        // Feedback visual
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            grabbedScale,
            scaleSpeed * Time.deltaTime
        );
    }

    private void FixedUpdate()
    {
        if (!isGrabbed || grabTarget == null)
            return;

        // Follow físico
        Vector3 toTarget = grabTarget.position - objectRb.position;
        objectRb.linearVelocity = toTarget * followSpeed;

        Quaternion newRotation = Quaternion.Slerp(
            objectRb.rotation,
            grabTarget.rotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        objectRb.MoveRotation(newRotation);
    }

    protected override void OnInteractEnd()
    {
        // Release consciente = lanzamiento
        if (isGrabbed)
            Release(true);
    }

    private void Release(bool launch)
    {
        isGrabbed = false;
        desyncTimer = 0f;

        objectRb.useGravity = true;

        if (physicalCollider != null)
            physicalCollider.enabled = true;

        StartCoroutine(ScaleBack());

        if (launch && grabTarget != null)
        {
            Vector3 launchDir = grabTarget.forward;
            objectRb.AddForce(launchDir * launchForce, ForceMode.Impulse);
        }

        grabTarget = null;
    }

    private Transform FindGrabTarget(Transform interactor)
    {
        return interactor
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "GrabTarget")
            ?? interactor;
    }

    private IEnumerator ScaleBack()
    {
        while (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                scaleSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.localScale = originalScale;
    }
}