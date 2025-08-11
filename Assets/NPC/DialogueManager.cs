using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;   // ⬅ 추가

public class DialogueManager : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public TMP_Text txt;                 // 출력 TMP
    public TypewriterEffect typewriter;  // 비워두면 자동으로 찾음

    [Header("Input")]
    public KeyCode nextKey = KeyCode.G;  // 진행 키

    [Header("Options")]
    public bool showSpeakerName = true;  // 화자명 표시
    public bool stopAtEnd = true;        // true면 마지막 줄에서 멈춤
    public bool hideAtEnd = true;        // 마지막 줄 후 UI 숨김

    [Header("Visibility (선택)")]
    public CanvasGroup canvasGroup;      // 있으면 이걸 숨김
    public GameObject rootToHide;        // 없으면 이 오브젝트를 숨김(미지정 시 자기 자신)

    [Serializable]
    public struct Line { public string speaker; [TextArea(2, 5)] public string text; }
    public Line[] lines;

    private int index = 0;

    void Awake()
    {
        if (!txt) { Debug.LogError("[DM] TMP_Text를 연결하세요."); enabled = false; return; }
        if (!typewriter) typewriter = txt.GetComponent<TypewriterEffect>() ?? GetComponent<TypewriterEffect>();

        if (lines == null || lines.Length == 0)
        {
            lines = new Line[] {
                new Line{ speaker="햄슨",   text="…일어나. 정신이 드는가, 친구?" },
                new Line{ speaker="플레이어", text="(흐릿하게) 여긴… 어디지?" },
                new Line{ speaker="햄슨",   text="우린 지금 지하철 내부에 있다. 뭔가 큰일이 일어난 모양이야." },
                new Line{ speaker="햄슨",   text="천천히 주위를 살펴봐. 무전기 너머로 계속 지켜보고 있겠네." },
                new Line{ speaker="플레이어", text="기억이 잘 나질 않아… 왜 여기에 있는 거지?" },
                new Line{ speaker="햄슨",   text="신문에서 보던 그 열차야. 아마도 우리가 그 순간 안에 들어온 걸세." },
            };
        }

        index = 0;
        ShowCurrent();
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey)) OnNext();
    }

    // XR/버튼/마우스 클릭 모두 여기로
    public void OnPointerClick(PointerEventData _) => OnNext();

    public void OnNext()
    {
        if (lines == null || lines.Length == 0) return;

        // 타자 중이면 스킵 먼저
        if (typewriter && typewriter.IsTyping)
        {
            typewriter.Skip(Format(lines[index]));
            return;
        }

        // 다음 줄로
        if (index < lines.Length - 1)
        {
            index++;
            ShowCurrent();
            return;
        }

        // 마지막 줄
        if (hideAtEnd) { HideUI(); return; }
        if (stopAtEnd) { return; }                 // 그대로 멈춤
        // 반복 모드(마지막 줄 반복)
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        string line = Format(lines[index]);
        if (typewriter) typewriter.Play(line);
        else txt.text = line;
    }

    private string Format(Line l)
    {
        if (showSpeakerName && !string.IsNullOrWhiteSpace(l.speaker))
            return $"<b>{l.speaker}</b>\n{l.text}";
        return l.text;
    }

    private void HideUI()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else if (rootToHide)
        {
            rootToHide.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
