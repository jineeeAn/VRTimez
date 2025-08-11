using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using static Clue_Id;

public class boy_text_TMP : MonoBehaviour, IPointerClickHandler
{
    private bool clueGiven = false;
    [Header("UI")]
    public TMP_Text txt_Dialogue;             // 출력용 TMP
    public TypewriterEffect typewriter;       // 비워두면 자동으로 찾음

    [Header("Input")]
    public KeyCode nextKey = KeyCode.G;       // 다음/스킵 키

    [Header("Loop Range (inclusive)")]
    public int loopStartIndex = 0;            // 0부터
    public int loopEndIndex = 2;              // 2까지 반복

    private int count = 0;
    private static string[] dialogue_boy;

    private void Awake()
    {
        dialogue_boy = new string[3];

        // 반복할 세 줄
        dialogue_boy[0] = "\n\n아저씨… 그 사람, 불 나기 전부터 저기 문 있는 데로 갔어요.";
        dialogue_boy[1] = "\n\n저기요! 저 사람, 아까부터 저 문 만지고 있었어요. 열려고 하는 것 같았어요.";
        dialogue_boy[2] = "\n\n불 나기 전인데… 이상하게 혼자 저쪽으로 막 달려갔어요.";
    }

    private void Start()
    {
        if (txt_Dialogue == null)
        {
            Debug.LogError("[boy_text_TMP] TMP_Text가 비어있습니다. Inspector에서 txt_Dialogue를 지정하세요.");
            enabled = false;
            return;
        }

        // Typewriter 자동 연결
        if (typewriter == null)
        {
            typewriter = txt_Dialogue.GetComponent<TypewriterEffect>();
            if (typewriter == null) typewriter = GetComponent<TypewriterEffect>();
        }

        // 루프 범위 안전화
        loopStartIndex = Mathf.Clamp(loopStartIndex, 0, dialogue_boy.Length - 1);
        loopEndIndex = Mathf.Clamp(loopEndIndex, 0, dialogue_boy.Length - 1);
        if (loopEndIndex < loopStartIndex) loopEndIndex = loopStartIndex;

        count = loopStartIndex;
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        if (!HasLine(count)) return;
        SafeSetText(dialogue_boy[count]);
    }

    private bool HasLine(int idx)
    {
        return dialogue_boy != null && idx >= 0 && idx < dialogue_boy.Length && dialogue_boy[idx] != null;
    }

    private void SafeSetText(string text)
    {
        if (typewriter != null) typewriter.Play(text);
        else txt_Dialogue.text = string.IsNullOrEmpty(text) ? string.Empty : text;
    }

    private void NextDialogue()
    {
        // 다음 인덱스로 이동
        count++;
        if (count > loopEndIndex) count = loopStartIndex;
        ShowCurrent();
        TryGiveClue();
    }
    private void TryGiveClue()
    {
        if (clueGiven) return;
        // 조건: 첫 줄을 한 번이라도 보여줬으면 지급(원하면 인덱스 바꿔도 OK)
        if (count == 1 || count == loopStartIndex)
        {
            ClueManager.I?.Add(ClueId.Boy_EmergencyDoor_BeforeFire);
            clueGiven = true;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(nextKey))
        {
            // 1) 타자 중이면 먼저 스킵
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.Skip(dialogue_boy[count]);
                return;
            }
            // 2) 그 다음 줄로
            NextDialogue();
        }
    }

    // 키 입력 대신 공통으로 호출할 메서드
    public void OnNext()
    {
        if (typewriter != null && typewriter.IsTyping)
            typewriter.Skip(dialogue_boy[count]);
        else
            NextDialogue();
    }

    // UI를 "터치/클릭"했을 때 호출됨 (XR Ray/Poke 모두 됨)
    public void OnPointerClick(PointerEventData eventData)
    {
        OnNext();
    }

}
