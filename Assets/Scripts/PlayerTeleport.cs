using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
    //같은 씬내 위치 이동(텔레포트)할 때 사용
    public Transform teleportTarget;

    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && teleportTarget != null)
        {
            other.transform.position = teleportTarget.position;
            other.transform.rotation = teleportTarget.rotation; 
        }
    }
}
