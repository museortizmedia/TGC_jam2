using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractionMoving : InteractiveObject
{
    [Header("Axis Constraints")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = false;
    [SerializeField] private bool followZ = true;

    [Header("Joint Settings")]
    [SerializeField] private float jointSpring = 5000f;
    [SerializeField] private float jointDamping = 100f;

    private Rigidbody objectRb;
    private Rigidbody interactorRb;

    private ConfigurableJoint joint;
    private RigidbodyConstraints originalConstraints;

    private void Awake()
    {
        objectRb = GetComponent<Rigidbody>();
        objectRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        ApplyAxisConstraints();
    }

    protected override void OnArrived() { }

    protected override void OnLeave()
    {
        if (joint != null)
            Release();
    }

    protected override void OnInteractStart()
    {
        if (currentInteractor == null)
            return;

        interactorRb = currentInteractor.GetComponent<Rigidbody>();
        if (interactorRb == null)
            return;

        originalConstraints = objectRb.constraints;

        ApplyAxisConstraints();
        CreateJoint();

    }

    protected override void OnInteract() { }

    protected override void OnInteractEnd()
    {
        Release();
    }

    private void CreateJoint()
    {
        joint = objectRb.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = interactorRb;

        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor =
            interactorRb.transform.InverseTransformPoint(objectRb.position);

        joint.xMotion = followX ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;
        joint.yMotion = followY ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;
        joint.zMotion = followZ ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;

        JointDrive drive = new JointDrive
        {
            positionSpring = jointSpring,
            positionDamper = jointDamping,
            maximumForce = Mathf.Infinity
        };

        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
    }

    private void ApplyAxisConstraints()
    {
        RigidbodyConstraints constraints = RigidbodyConstraints.FreezeRotation;

        if (!followX) constraints |= RigidbodyConstraints.FreezePositionX;
        if (!followY) constraints |= RigidbodyConstraints.FreezePositionY;
        if (!followZ) constraints |= RigidbodyConstraints.FreezePositionZ;

        objectRb.constraints = constraints;
    }

    private void Release()
    {
        if (joint != null)
            Destroy(joint);

        objectRb.constraints = originalConstraints;
        interactorRb = null;
    }
}