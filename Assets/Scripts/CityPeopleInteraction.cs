using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TalkButtonHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private float dialogueDuration = 3f;

    private Button talkButton;

    void Start()
    {

        talkButton = GameObject.Find("talkButton").GetComponent<Button>();
        if (talkButton != null)
        {
            talkButton.onClick.AddListener(OnTalkClicked);
        }
    }

    public void OnTalkClicked()
    {
        Debug.Log("🗨️ 말 걸기 버튼 클릭됨");

        // 버튼 숨기기
        if (talkButton != null)
        {
            talkButton.gameObject.SetActive(false);
        }

        // 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("triggerTalk");
        }

        // 대사창 활성화 + 일정 시간 뒤 자동 비활성화
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
            StartCoroutine(HideDialogueAfterSeconds(dialogueDuration));
        }
    }

    private IEnumerator HideDialogueAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        // 대사창 끄기
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }

        // 버튼 다시 보이기
        if (talkButton != null)
        {
            talkButton.gameObject.SetActive(true);
        }
    }
}
