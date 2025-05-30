using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class MlA_Movement : Agent
{

    public GameObject sphere;
    public GameObject plane;
    public Material winMat, loseMat;
    
    public float cooldown = 2.0f ; // Cooldown for punching the sphere
    public float moveSpeed = 5f; // Speed of the agent's movement
    public override void OnEpisodeBegin()
    {
        // Reset the agent's position and state at the beginning of each episode

        //make the character start at the center of the plane 

        // predict a random position for the sphere within the plane bounds
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect observations about the environment here
        //observe distance/angel between character and spheres local position 

        sensor.AddObservation(Vector3.Distance(transform.localPosition, sphere.localPosition));
        sensor.AddObservation(Vector3.Angle(transform.forward, sphere.position - transform.position) / 180f);
        //wait a while for another punch 
        sensor.AddObservation(Mathf.Clamp01(cooldown));

        //animation rotine and prediction
        // walk , turn left, turn right , punch , idle 

    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Define how the agent should respond to actions here
        // movement on y = 0
        float x= actions.DiscreteActions[0];
        float z = actions.DiscreteActions[1];

        Vector3 pos = new Vector3(x, 0, z);

        transform.Translate(pos * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < 0)// fell off 
        {
            SetReward(-1f);
        }
        // punich for puncjing without colliding with sphere 
        // punich for punching sevral times after each other without waiting for cooldown
        // reward if calculated distance is becomming smaller, getting closer to the sphere
        // reward for inverse angle, the anglw is correct so its inverse is indicationg closeness to the sphere

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name=="Sphere")
        {
            // Punch the sphere
          
            SetReward(1f); // Reward for punching the sphere
            // set sphere color with win material to resemble win 
            plane.GetComponent<Renderer>().material = winMat;
            EndEpisode();

        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Define manual control for testing purposes here
        //press  take control manually the animation  to move and able to punch manually to effect the trianing 

        // also check if the sphere fell down 
          private void OnCollisionEnter(Collision collision)
        {
        if (collision.gameObject.name == "Sphere")
        {
            SetReward(1f);
            plane.GetComponent<Renderer>().material = winMat;
            EndEpisode();
        }
    }



}