using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleportTrigger : MonoBehaviour
{
    //이건 다른 씬으로 텔레포트할 때 사용
    public string targetSceneName;

    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
