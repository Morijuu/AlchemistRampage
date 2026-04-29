using UnityEngine;

public class UiFollowNoRotate : MonoBehaviour
{
    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}