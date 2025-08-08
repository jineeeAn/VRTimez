using UnityEngine;

public class FloatingUIFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;  // 따라갈 대상 (VR 카메라)
    public float distance = 2f;        // 카메라와의 거리
    void LateUpdate()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPosition = cameraTransform.position + forward * distance;


        targetPosition.y += -1f;
        targetPosition.x += -2f;

        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(forward);
    }

}