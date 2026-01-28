using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target.position.y > transform.position.y)
        {
            Vector3 newPos = new Vector3(0f, target.position.y, transform.position.z);
            transform.position = newPos;
        }
    }
}