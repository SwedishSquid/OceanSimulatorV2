using UnityEngine;

public class Push : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float magnitude = 1f;
    public Vector3 direction = Vector3.right;

    void Start()
    {
        GetComponent<Rigidbody>().AddForce(direction * magnitude, ForceMode.Impulse);
    }
}
