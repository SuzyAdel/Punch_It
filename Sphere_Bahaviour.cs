using UnityEngine;

public class Sphere_Bahaviour : MonoBehaviour
{
    public float amplitude = 1f;
    public float frequency = 1f;
    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.localPosition; // Store the initial position of the sphere
    }

    // Update is called once per frame
    void Update()
    {
        // Update the position of the sphere to create a bouncing effect
        transform.localPosition = startPos + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
    }
}
