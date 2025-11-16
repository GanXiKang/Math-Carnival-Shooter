using UnityEngine;

public class ImageZRotation : MonoBehaviour
{
    public float angle = 10f;
    public float speed = 10f;

    private float startZ;

    void Start()
    {
        startZ = transform.localEulerAngles.z;
    }

    void Update()
    {
        float z = startZ + Mathf.Sin(Time.time * speed) * angle;

        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            z
        );
    }
}
