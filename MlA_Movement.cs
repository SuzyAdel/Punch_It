using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class MlA_Movement : Agent
{
    [Header("References")]
    public GameObject sphere; // target to punch 
    public GameObject plane;
    public Animator animator;
    public Material winMat, loseMat;
    public Transform punchOrigin; // Assign the right hand that punches 

    [Header("Settings")]
    public float cooldown = 0.5f; // too long and low reward cuases ignore punching thus dwcreased 
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f; // degrees per second
    public float punchRange = 1f; // distance to punch the sphere
    public float punchAngleThreshold = 30f; // angle in degrees to consider a successful punch

    private float currentCooldown = 0f;
    private Rigidbody rb;

    void Start()
    {
        // Ensure the animator is assigned, Cache Rigidbody and freeze unwanted rotations
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Debug reference assignments
        if (sphere == null) Debug.LogError("Sphere reference not assigned!");
        if (animator == null) Debug.LogError("Animator reference not assigned!");
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position
        float planeSize = plane.transform.localScale.x * 10f; // Size of the plane 
        float agentHeight = GetComponent<CapsuleCollider>().height;
        float adjustedY = plane.transform.position.y + agentHeight / 2f; // diveded 2 to assure even height on the plane

        float halfSize = planeSize / 2f - 1f; // Calculate playable radius (with 1 unit margin)

        transform.position = new Vector3(
            plane.transform.position.x + Random.Range(-halfSize, halfSize),  // Random X position within margin
            adjustedY,
            plane.transform.position.z + Random.Range(-halfSize, halfSize)   // Random Z position within margin
        );
        transform.rotation = Quaternion.identity;

        // Reset sphere position
        //Debug.Log("Episode started: repositioning sphere");//looks in the smae postion but acctually changing places 

        float randomHeight = Random.Range(0.5f, 1.5f); // Random height between 0.5 and 1.5 units above plane

        sphere.transform.position = new Vector3(
            plane.transform.position.x + Random.Range(-halfSize, halfSize),  // Random X position
            plane.transform.position.y + randomHeight,  // Y position with variation
            plane.transform.position.z + Random.Range(-halfSize, halfSize)   // Random Z position
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
        sensor.AddObservation(Mathf.Clamp01(distance / maxDistance)); 

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
            //AddReward(-0.02f); // Small penalty for not moving , increased it from -0.01f to -0.07f to encourage movement
            // decreased to 0.2 becase 0.7 forced it to deviate weater its close or  not
            //REMOVED , CONFUSES MODEL , forces any movement 
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
                //AddReward(-0.02f); // Penalty for inaccurat punch, soften from -0.1f to -0.02f to encorage learning
                //discourage strategic pauses!! 
            }
        }
        else if (actions.DiscreteActions[2] == 0 && currentCooldown > 0f)
        {
            AddReward(-0.05f); // Penalty for punching during cooldown, keept high as i increased duration 
            // This encourages the agent to wait for cooldown to finish before punching again, too harsh
        }

        // Update cooldown decrement
        if (currentCooldown > 0f) currentCooldown -= Time.deltaTime;

        // Falling check 
        if (transform.position.y < -1f)
        {
            plane.GetComponent<Renderer>().material = loseMat;
            SetReward(-15f); // Increased penalty for falling to make it more powerfull
            EndEpisode();
        }

        // Continuous rewards
        float normDistance = Mathf.Clamp01(Vector3.Distance(transform.position, sphere.transform.position) / (plane.transform.localScale.x * 10f));
        AddReward(0.5f * (1 - normDistance)); // Distance reward , increased from max 0.1 to 0.5 

        float normAngle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position) / 180f;
        AddReward(0.5f * (1 - normAngle)); // Angle reward..
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Null check critical components first
        if (collision == null || collision.gameObject == null ||
            sphere == null || animator == null || plane == null)
        {
            Debug.LogWarning("Collision system missing references!");
            return;
        }

        // 2. Verify we hit the sphere (using direct reference comparison)
        if (collision.gameObject != sphere) return;

        // 3. Safe animation state check
        try
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            // 4. Only proceed during active punch frames
            if (state.IsName("Punch") && state.normalizedTime < 0.8f) //short punch animation duration, relaxed from 0.3 to 0.8f
            {
                // 5. Null-check before material change
                var planeRenderer = plane.GetComponent<Renderer>();
                if (planeRenderer != null)
                {
                    planeRenderer.material = winMat;
                }
                else
                {
                    Debug.LogWarning("Missing plane renderer!");
                }

                // 6. Add force if rigidbody exists
                if (collision.rigidbody != null)
                {
                    collision.rigidbody.AddForce(transform.forward * 10f, ForceMode.Impulse);
                }

                SetReward(15f); //increased reward to make it stand out , instead of 1 --> 15f
                EndEpisode();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Animation error: {e.Message}");
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
}// repo_link: https://github.com/SuzyAdel/Punch_It

// I wanted to use curriculum learning:
// ex Phase 1: Fixed target position, no cooldown

//Phase 2: Random target, no cooldown

//Phase 3: Add cooldown

//Phase 4: Add animation constraints

// but for some reason the reusme never worked so i had to restart each time 
