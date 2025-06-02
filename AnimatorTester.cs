using UnityEngine;

public class AnimatorTestController : MonoBehaviour
{
    [Header("References")]
    public GameObject sphere;
    public GameObject plane;
    public Animator animator;
    public Material winMat, loseMat;
    public Transform punchOrigin;
    public Renderer groundRenderer;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;
    public float punchRange = 1.5f;
    public float punchAngleThreshold = 30f;
    public float cooldown = 2f;

    private float currentCooldown = 0f;

    void Start()
    {
        ResetAnimatorStates();
    }

    void Update()
    {
        HandleMovement();
        HandlePunch();
        CooldownTick();
        FallCheck();
    }

    void HandleMovement()
    {
        bool walked = false;
        bool turned = false;

        // Walking
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);
            walked = true;
        }

        // Rotation
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            animator.SetBool("Left", true);
            animator.SetBool("Right", false);
            turned = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            animator.SetBool("Right", true);
            animator.SetBool("Left", false);
            turned = true;
        }

        // Reset if not walking or turning
        if (!walked)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", true);
        }

        if (!turned)
        {
            animator.SetBool("Left", false);
            animator.SetBool("Right", false);
        }
    }

    void HandlePunch()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentCooldown <= 0f)
        {
            ResetPunchTrigger();
            animator.SetTrigger("Punch");
            currentCooldown = cooldown;

            bool hit = SimulatePunchHit();
            groundRenderer.material = hit ? winMat : loseMat;
            Debug.Log(hit ? "💥 Punch HIT" : "❌ Punch MISS");
        }
    }

    bool SimulatePunchHit()
    {
        float distance = Vector3.Distance(punchOrigin.position, sphere.transform.position);
        float angle = Vector3.Angle(transform.forward, sphere.transform.position - transform.position);
        return distance < punchRange && angle < punchAngleThreshold;
    }

    void CooldownTick()
    {
        if (currentCooldown > 0f)
            currentCooldown -= Time.deltaTime;
    }

    void FallCheck()
    {
        if (transform.position.y < -1f)
        {
            Debug.Log("🪂 Agent fell. Resetting...");
            transform.position = new Vector3(0f, 1f, 0f);
            transform.rotation = Quaternion.identity;
            groundRenderer.material = loseMat;
        }
    }

    void ResetAnimatorStates()
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Walk", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);
        ResetPunchTrigger();
    }

    void ResetPunchTrigger()
    {
        animator.ResetTrigger("Punch");
    }
}
