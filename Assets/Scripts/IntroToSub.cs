using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroToSub : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("SubwayScene");
    }
}
