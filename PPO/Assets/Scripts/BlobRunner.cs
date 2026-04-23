using UnityEngine;

// Controls a bipedal blob character: torso + thighs + shins, driven by 4 HingeJoint motors.
// Attach to the root GameObject that contains all body-part Rigidbodies as children.
public class BlobRunner : MonoBehaviour
{
    [Header("Bodies")]
    public Rigidbody torso;
    public Rigidbody leftThigh;
    public Rigidbody rightThigh;
    public Rigidbody leftShin;
    public Rigidbody rightShin;

    [Header("Joints")]
    public HingeJoint leftHip;
    public HingeJoint rightHip;
    public HingeJoint leftKnee;
    public HingeJoint rightKnee;

    [Header("Foot Contacts")]
    public GroundContact leftFootContact;
    public GroundContact rightFootContact;

    [Header("Motor Settings")]
    public float maxMotorForce = 200f;
    public float maxMotorVelocity = 180f;   // degrees/sec at action = 1

    struct BodySnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    Rigidbody[] _allBodies;
    BodySnapshot[] _initialPoses;

    void Awake()
    {
        _allBodies = GetComponentsInChildren<Rigidbody>();
        _initialPoses = new BodySnapshot[_allBodies.Length];
        for (int i = 0; i < _allBodies.Length; i++)
            _initialPoses[i] = new BodySnapshot
            {
                position = _allBodies[i].transform.position,
                rotation = _allBodies[i].transform.rotation
            };
    }

    // actions are in [-1, 1]; positive = forward swing / extension
    public void ApplyActions(float leftHipA, float rightHipA, float leftKneeA, float rightKneeA)
    {
        DriveMotor(leftHip,  leftHipA);
        DriveMotor(rightHip, rightHipA);
        DriveMotor(leftKnee,  leftKneeA);
        DriveMotor(rightKnee, rightKneeA);
    }

    public void Reset()
    {
        for (int i = 0; i < _allBodies.Length; i++)
        {
            _allBodies[i].transform.SetPositionAndRotation(
                _initialPoses[i].position,
                _initialPoses[i].rotation);
            _allBodies[i].linearVelocity = Vector3.zero;
            _allBodies[i].angularVelocity = Vector3.zero;
        }
    }

    void DriveMotor(HingeJoint joint, float action)
    {
        JointMotor motor = joint.motor;
        motor.targetVelocity = action * maxMotorVelocity;
        motor.force = maxMotorForce;
        joint.motor = motor;
        joint.useMotor = true;
    }
}
