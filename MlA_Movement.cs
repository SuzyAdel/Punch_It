using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MlA_Movement : Agent
{
    public GameObject sphere;
    public GameObject plane;
    public GameObject terrian;
    public Animator animator;
    public Material winMat, loseMat;
    
    public float cooldown = 2.0f; // Cooldown for punching the sphere
    public float moveSpeed = 5f; // Speed of the agent's movement
    public float rotationSpeed = 100f; // Speed of rotation

    private float currentCooldown = 0f;

    public override void OnEpisodeBegin()
    {
        // get plane size from the attached game object 
        int planeSize = (int)plane.transform.localScale.x; // Assuming the plane is square, use x scale

        // Reset the agent's position and state at the beginning of each episode
        //make the character start at the center of the plane 
        transform.localPosition = new Vector3(0, 0.5f, 0); // Center of plane with slight y offset

        transform.rotation = Quaternion.identity; // Face forward
        // Reset cooldown timer
        currentCooldown = 0f;
        // Set random position for the sphere within the plane bounds
        float randomX = Random.Range(-planeSize / 2, planeSize / 2);
        float randomZ = Random.Range(-planeSize / 2, planeSize / 2);
        sphere.transform.localPosition = new Vector3(randomX, 0.5f, randomZ);

        // Reset materials
        plane.GetComponent<Renderer>().material = loseMat;

        // Reset animation
        animator.SetBool("Idle", true);
        animator.SetBool("Walk", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);
        animator.SetBool("Punch", false);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect observations about the environment here
        //observe distance/angel between character and spheres local position 

        sensor.AddObservation(Vector3.Distance(transform.localPosition, sphere.transform.localPosition));
        sensor.AddObservation(Vector3.Angle(transform.forward, sphere.transform.position - transform.position) / 180f);
        //wait a while for another punch 
        sensor.AddObservation(Mathf.Clamp01(currentCooldown / cooldown));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Define how the agent should respond to actions 
        // Process movement actions
        int moveAction = actions.DiscreteActions[0]; // Branch 0: Walk forward (0=idle, 1=walk)
        int turnAction = actions.DiscreteActions[1]; // Branch 1: Turn (0=left, 1=right, 2=no turn)
        int punchAction = actions.DiscreteActions[2]; // Branch 2: Punch (0=trigger punch)

        // Handle movement
        if (moveAction == 1)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);

        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", true);

            //  Discsrdge staying still 
            AddReward(-0.01f); // Small penalty for not moving
        }

        // Handle turning
        if (turnAction == 0) // Left
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            animator.SetBool("Left", true);
            animator.SetBool("Right", false);
        }
        else if (turnAction == 1) // Right
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            animator.SetBool("Right", true);
            animator.SetBool("Left", false);
        }
        else // No turn
        {
            animator.SetBool("Left", false);
            animator.SetBool("Right", false);
        }

        // Handle punching
        if (punchAction == 0 && currentCooldown <= 0f)
        {
            animator.SetBool("Punch", true);
            currentCooldown = cooldown;

            // Check if we might hit the sphere (simplified check)
            float distance = Vector3.Distance(transform.position, sphere.transform.position);
            float angle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position);

            if (distance < 2f && angle < 30f) // Within punching range and angle
            {
                // We'll let the actual collision handle the reward
            }
            else
            {
                // Penalize for punching when not properly aligned
                AddReward(-0.1f);//-0.5f is too high, so I used -0.1f to avoid harsh penalties
            }
        }
        else
        {
            animator.SetBool("Punch", false);

            // Penalize for punching during cooldown
            if (punchAction == 0 && currentCooldown > 0f)
            {
                AddReward(-0.1f);// -0.5f is too high, so I used -0.1f to avoid harsh penalties
            }
        }

        // Update cooldown timer
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        // Fell off check
        if (transform.position.y < -1f) // fell off 
        {
            SetReward(-1f);
            EndEpisode();
        }

        // Reward for getting closer to target
        // divide on smaller vlues means bigger value 
        // inverse distance reward, this may lead to explosion so normlize 
        float currentDistance = Vector3.Distance(transform.position, sphere.transform.position);
        //AddReward(0.1f / currentDistance);
        float maxReward = 0.1f;
        float maxDistance = 10f; 
        AddReward(maxReward * (1f - Mathf.Clamp01(currentDistance / maxDistance)));


        // Reward for better angle to target
        float angleToTarget = Vector3.Angle(transform.forward, sphere.transform.position - transform.position);
        AddReward(0.5f * (1f - angleToTarget / 180f)); // Normalized angle reward
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Sphere")
        {
            // Check if we're in punching animation
            if (animator.GetBool("Punch"))
            {
                // Punch the sphere
                SetReward(1f); // Reward for punching the sphere
                // set sphere color with win material to resemble win 
                plane.GetComponent<Renderer>().material = winMat;
                EndEpisode();
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Define manual control for testing purposes here
        //press  take control manually the animation  to move and able to punch manually to effect the trianing 
        var discreteActions = actionsOut.DiscreteActions;

        // Movement (W key)
        discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : 0;

        // Turning (A and D keys)
        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[1] = 0; // Left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[1] = 1; // Right
        }
        else
        {
            discreteActions[1] = 2; // No turn
        }

        // Punch (Space key)
        discreteActions[2] = Input.GetKey(KeyCode.Space) ? 0 : 1;
    }
}
