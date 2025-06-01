using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MlA_Movement : Agent
{
    [Header("References")]
    public GameObject sphere; // target to punch 
    public GameObject plane;
    public Animator animator;
    public Material winMat, loseMat;
    public Transform punchOrigin; // Assign the  right hand that  pumches 

    [Header("Settings")]
    public float cooldown = 2.0f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f; // degrees per second
    public float punchRange = 4f; // distance to punch the sphere
    public float punchAngleThreshold = 30f; // angle in degrees to consider a successful punch

    private float currentCooldown = 0f;
    private Rigidbody rb;

    void Start()
    {
        // Ensure the animator is assigned, Cache Rigidbody and freeze unwanted rotations
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position
        float planeSize = plane.transform.localScale.x * 10f; // Size of the plane 
        float agentHeight = GetComponent<CapsuleCollider>().height;
        float adjustedY = plane.transform.position.y + agentHeight / 2f; // diveded 2 to assure even height on the plane

        transform.position = new Vector3(
            plane.transform.position.x,
            adjustedY,
            plane.transform.position.z
        );
        transform.rotation = Quaternion.identity;

        // Reset sphere position
        float halfSize = planeSize / 2f - 1f; // Margin from edges
        sphere.transform.position = new Vector3(
            Random.Range(-halfSize, halfSize),
            plane.transform.position.y + 0.5f,
            Random.Range(-halfSize, halfSize)
        );

        // Reset materials and cooldown
        //plane.GetComponent<Renderer>().material = loseMat; // Reset to loseMat to indicate start false -ve 
        currentCooldown = 0f;
        ResetAllTriggers();
        animator.SetBool("Idle", true);
    }

    void ResetAllTriggers()  // Clear all animation states to prevent conflicts
    {
        animator.ResetTrigger("Punch");
        animator.SetBool("Idle", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);
        animator.SetBool("Walk", false);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalized distance observation (0 -1)
        float maxDistance = plane.transform.localScale.x * 10f;
        float distance = Vector3.Distance(transform.position, sphere.transform.position);
        sensor.AddObservation(Mathf.Clamp01(distance / maxDistance)); // 

        // Normalized angle observation (0-1)
        float angle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position);
        sensor.AddObservation(angle / 180f);

        // Current cooldown state (0 = ready, 1 = full cooldown)
        sensor.AddObservation(Mathf.Clamp01(currentCooldown / cooldown));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Movement 
        if (actions.DiscreteActions[0] == 1) // Walk
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);
        }
        else // Idle ,no movement
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", true);
            AddReward(-0.01f); // Small penalty for not moving
        }

        // Rotation
        switch (actions.DiscreteActions[1])
        {
            case 0: // Left
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
                animator.SetBool("Left", true);
                animator.SetBool("Right", false);
                break;
            case 1: // Right
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                animator.SetBool("Right", true);
                animator.SetBool("Left", false);
                break;
            default: // No turn
                animator.SetBool("Left", false);
                animator.SetBool("Right", false);
                break;
        }

        // Punching
        if (actions.DiscreteActions[2] == 0 && currentCooldown <= 0f)
        {
            animator.ResetTrigger("Punch");
            animator.SetTrigger("Punch");
            currentCooldown = cooldown;

            // Check punch accuracy, using distance and angle ranges 
            float distance = Vector3.Distance(punchOrigin.position, sphere.transform.position);
            float angle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position);

            if (distance < punchRange && angle < punchAngleThreshold)
            {
                // Reward will be handled in OnCollisionEnter
            }
            else
            {
                AddReward(-0.1f); // Penalty for inaccurat punch
            }
        }
        else if (actions.DiscreteActions[2] == 0 && currentCooldown > 0f)
        {
            AddReward(-0.1f); // Penalty for punching during cooldown
        }

        // Update cooldown decrement
        if (currentCooldown > 0f) currentCooldown -= Time.deltaTime;

        // Falling check 
        if (transform.position.y < -1f)
        {
            plane.GetComponent<Renderer>().material = loseMat;
            SetReward(-1f);
            EndEpisode();
        }

        // Continuous rewards
        float normDistance = Mathf.Clamp01(Vector3.Distance(transform.position, sphere.transform.position) / (plane.transform.localScale.x * 10f));
        AddReward(0.1f * (1 - normDistance)); // Distance reward

        float normAngle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position) / 180f;
        AddReward(0.05f * (1 - normAngle)); // Angle reward
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == sphere)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Punch") && state.normalizedTime < 0.3f)
            {
                SetReward(1f);
                plane.GetComponent<Renderer>().material = winMat;
                collision.rigidbody.AddForce(transform.forward * 10f, ForceMode.Impulse);
                EndEpisode();
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        // Movement, Forward or Idle W
        discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : 0;

        // Rotation , Left or Right A/D
        if (Input.GetKey(KeyCode.A))
            discreteActions[1] = 0;
        else if (Input.GetKey(KeyCode.D))
            discreteActions[1] = 1;
        else discreteActions[1] = 2;

        // Punch, space bar
        discreteActions[2] = Input.GetKey(KeyCode.Space) ? 0 : 1;
    }
}
