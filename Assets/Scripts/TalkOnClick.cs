using UnityEngine;
using System.Collections;

public class TalkOnClick : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private float dialogueDuration = 3f;

    void Update()
    {
        // 마우스 왼쪽 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 클릭된 오브젝트가 자기 자신인지 확인
                if (hit.transform == transform)
                {
                    OnCharacterClicked();
                }
            }
        }
    }

    private void OnCharacterClicked()
    {
        Debug.Log("👆 캐릭터 클릭됨");

        // 애니메이션 실행
        if (animator != null)
        {
            animator.SetTrigger("triggerTalk");
        }

        // 대사창 활성화 및 자동 종료
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
            StartCoroutine(HideDialogueAfterSeconds(dialogueDuration));
        }
    }

    private IEnumerator HideDialogueAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
    }
}
