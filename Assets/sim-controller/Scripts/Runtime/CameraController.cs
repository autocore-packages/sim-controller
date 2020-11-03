using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : UnitySingleton<CameraController>
{
    public bool followCenterOfMass = true;
    public Transform target;
    Vector3 m_localTargetOffset;
    Rigidbody m_targetRigidbody;
    public LayerMask collisionMask = Physics.DefaultRaycastLayers;

    #region CameraData

    public float distance = 10.0f;
    public float height = 5.0f;
    public float viewHeightRatio = 0.5f;      // Look above the target (height * this ratio)
    public bool lookBehind = false;
    [Space(5)]
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;
    [Space(5)]
    public bool followVelocity = true;
    public float velocityDamping = 5.0f;


    WheelDrive m_vehicle;
    public Camera m_camera;
    Transform m_transform;

    Vector3 m_targetOffset;

    Vector3 m_smoothLastPos = Vector3.zero;
    Vector3 m_smoothVelocity = Vector3.zero;
    float m_smoothTargetAngle = 0.0f;

    float m_selfRotationAngle;
    float m_selfHeight;
    #endregion

    private void OnEnable()
    {
        m_camera = GetComponent<Camera>();
        AdquireTarget();
        ComputeTargetOffset();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void LateUpdate()
    {
        ComputeTargetOffset();
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (target == null) return;
        Vector3 updateVolocity = (target.position + m_targetOffset - m_smoothLastPos) / Time.deltaTime;
        if (lookBehind) updateVolocity = -updateVolocity;
        m_smoothLastPos = target.position + m_targetOffset;

        updateVolocity.y = 0;
        if (updateVolocity.magnitude > 1f)
        {
            m_smoothVelocity = Vector3.Lerp(m_smoothVelocity, updateVolocity, velocityDamping * Time.deltaTime);
            m_smoothTargetAngle = Mathf.Atan2(m_smoothVelocity.x, m_smoothVelocity.z) * Mathf.Rad2Deg;
        }
        if (!followVelocity) m_smoothTargetAngle = target.eulerAngles.y;

        float wantedHeight = target.position.y + m_targetOffset.y + height;
        m_selfRotationAngle = Mathf.LerpAngle(m_selfRotationAngle, m_smoothTargetAngle, rotationDamping * Time.deltaTime);
        m_selfHeight = Mathf.Lerp(m_selfHeight, wantedHeight, heightDamping * Time.deltaTime);
        Quaternion currentRotation = Quaternion.Euler(0, m_selfRotationAngle, 0);

        Vector3 selfPos = target.position + m_targetOffset;
        selfPos -= currentRotation * Vector3.forward * distance;
        selfPos.y = m_selfHeight;

        Vector3 lookAtTarget = target.position + m_targetOffset + Vector3.up * height * viewHeightRatio;

        if (m_vehicle != null)
        {
            if (m_camera != null)
            {
                Vector3 origin = lookAtTarget;
                Vector3 path = selfPos - lookAtTarget;
                Vector3 direction = path.normalized;
                float rayDistance = path.magnitude - m_camera.nearClipPlane;
                float radius = m_camera.nearClipPlane * Mathf.Tan(m_camera.fieldOfView * Mathf.Deg2Rad * 0.5f) + 0.1f;

                selfPos = origin + direction * m_vehicle.SphereRaycastOthers(origin, direction, radius, rayDistance, collisionMask);
            }
            else
            {
                selfPos = m_vehicle.RaycastOthers(lookAtTarget,selfPos,collisionMask);
            }
        }
        m_transform.position = selfPos;
        m_transform.LookAt(lookAtTarget);
    }

    public void Reset()
    {
        if (target == null) return;
        m_vehicle = target.GetComponent<WheelDrive>();

        m_smoothLastPos = target.position + m_targetOffset;
        m_smoothVelocity = target.forward * 2f;
        m_smoothTargetAngle = target.eulerAngles.y;

        m_selfRotationAngle = m_transform.eulerAngles.y;
        m_selfHeight = m_transform.position.y;

    }
    private void AdquireTarget()
    {
        if (followCenterOfMass && target != null)
        {
            m_targetRigidbody = target.GetComponent<Rigidbody>();
            if (m_targetRigidbody)
                m_localTargetOffset = m_targetRigidbody.centerOfMass;
        }
        else
        {
            m_targetRigidbody = null;
        }
        Reset();
    }
    private void ComputeTargetOffset()
    {
        if (followCenterOfMass && m_targetRigidbody != null)
        {
            m_targetOffset = target.TransformDirection(m_localTargetOffset);
        }
        else
        {
            m_targetOffset = Vector3.zero;
        }
    }
}
