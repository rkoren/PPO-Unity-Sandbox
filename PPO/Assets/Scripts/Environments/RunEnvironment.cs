using UnityEngine;

// 100m sprint environment. The blob must run as far forward (Z axis) as possible
// before falling over or running out of time.
//
// Scene setup:
//   - Attach to a GameObject alongside a PhysicsManager / Bootstrap
//   - Assign a BlobRunner prefab instance to `runner`
//   - Tag the ground plane "Ground"
//   - Forward direction is +Z

public class RunEnvironment : Environment
{
    public BlobRunner runner;

    [Header("Episode Settings")]
    public float targetDistance = 100f;
    public float fallHeightThreshold = 0.5f;   // torso y below this = fallen
    public int maxSteps = 4000;

    int _stepCount;
    float _startZ;

    public override void Start()
    {
        _startZ = runner.torso.transform.position.z;
        ResetEnvironment();
    }

    public override void Step(float deltaTime)
    {
        if (!isRunning) return;

        float previousReward = episode.totalReward;

        Timestep timestep = new Timestep();
        timestep.state = StateTensor();
        PPOAgent.ActionTuple tuple = ActionTensor(timestep.state);
        timestep.action = tuple.action;
        timestep.e = tuple.e;
        timestep.logProb = tuple.logProb;

        runner.ApplyActions(
            timestep.action.values[0, 0, 0],
            timestep.action.values[1, 0, 0],
            timestep.action.values[2, 0, 0],
            timestep.action.values[3, 0, 0]);

        float forwardVel = runner.torso.linearVelocity.z;
        float uprightness = Mathf.Max(0f, Vector3.Dot(runner.torso.transform.up, Vector3.up));
        reward = forwardVel * uprightness * deltaTime;
        episode.totalReward += reward;
        timestep.reward = reward;

        _stepCount++;
        episode.length++;
        episode.samples.Add(timestep);
        episode.values.Add(agent.Value(timestep.state));

        float distanceTraveled = runner.torso.transform.position.z - _startZ;
        bool fallen  = runner.torso.transform.position.y < fallHeightThreshold;
        bool finished = distanceTraveled >= targetDistance;
        bool timedOut = _stepCount >= maxSteps;

        if (fallen || finished || timedOut)
        {
            EndEpisode();
            return;
        }

        base.Step(deltaTime);
    }

    public override void ResetEnvironment()
    {
        runner.Reset();
        _stepCount = 0;
        base.ResetEnvironment();
    }

    public override Matrix StateTensor()
    {
        Matrix state = new Matrix(16, 1, 1);
        int i = 0;

        Vector3 vel = runner.torso.linearVelocity;

        // Torso kinematics
        state.values[i++, 0, 0] = Mathf.Clamp(runner.torso.transform.position.y / 1.2f - 1f, -1f, 1f);
        state.values[i++, 0, 0] = Mathf.Tanh(vel.z / 5f);
        state.values[i++, 0, 0] = Mathf.Tanh(vel.x / 3f);
        state.values[i++, 0, 0] = Mathf.Tanh(vel.y / 3f);

        // Torso orientation
        state.values[i++, 0, 0] = Mathf.Clamp(runner.torso.transform.forward.y, -1f, 1f); // pitch
        state.values[i++, 0, 0] = Mathf.Clamp(runner.torso.transform.up.x,     -1f, 1f); // roll

        // Joint angles (HingeJoint.angle is in degrees; divide by 90 to get approx [-1, 1])
        state.values[i++, 0, 0] = Mathf.Clamp(runner.leftHip.angle  / 90f, -1f, 1f);
        state.values[i++, 0, 0] = Mathf.Clamp(runner.rightHip.angle / 90f, -1f, 1f);
        state.values[i++, 0, 0] = Mathf.Clamp(runner.leftKnee.angle  / 90f, -1f, 1f);
        state.values[i++, 0, 0] = Mathf.Clamp(runner.rightKnee.angle / 90f, -1f, 1f);

        // Limb angular velocities
        state.values[i++, 0, 0] = Mathf.Tanh(runner.leftThigh.angularVelocity.x  / 5f);
        state.values[i++, 0, 0] = Mathf.Tanh(runner.rightThigh.angularVelocity.x / 5f);
        state.values[i++, 0, 0] = Mathf.Tanh(runner.leftShin.angularVelocity.x   / 5f);
        state.values[i++, 0, 0] = Mathf.Tanh(runner.rightShin.angularVelocity.x  / 5f);

        // Foot ground contact
        state.values[i++, 0, 0] = runner.leftFootContact.isGrounded  ? 1f : 0f;
        state.values[i++, 0, 0] = runner.rightFootContact.isGrounded ? 1f : 0f;

        return state;
    }

    public override PPOAgent.ActionTuple ActionTensor(Matrix state)
    {
        return agent.SelectAction(ref state);
    }
}
