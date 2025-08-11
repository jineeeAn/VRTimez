using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Clue_Id;

public class passenger_text : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text txt_Dialogue;
    public TypewriterEffect typewriter;   // 비워두면 자동 탐색

    [Header("Input")]
    public KeyCode nextKey = KeyCode.G;

    private static string[] lines;
    private int index = 0;

    // 이 NPC의 각 대사 라인과 연결된 단서들
    private Dictionary<int, ClueId[]> clueByLine;
    private HashSet<ClueId> _given = new(); // 이미 지급된 단서

    void Awake()
    {
        lines = new string[] {
            // 0
            "\n\n그 사람, 계속 시계를 보며 메모를 하고 있었어요.",
            // 1
            "\n\n메모는 짧게 적었고, 뭔가 시간을 맞추는 듯한 느낌이었죠.",
            // 2
            "\n\n정확히 몇 시 몇 분 같은 걸 확인하는 모습이었어요.",
            // 3
            "\n\n우발적으로 보이진 않았습니다. 뭔가를 기다리거나 맞추는 듯했거든요.",
            // 4 (가이드)
            "\n\n만약 CCTV가 있다면, 특정 시간대에 그 사람이 어디로 움직였는지 확인해보세요."
        };

        // 줄별 단서 매핑
        clueByLine = new Dictionary<int, ClueId[]> {
            { 0, new[]{ ClueId.Passenger_WatchMemo } },   // 시계+메모 목격
            { 1, new[]{ ClueId.Passenger_TimingHint } },  // 타이밍/시간 맞춤 정황
            // 필요하면 2,3에도 같은 단서 반복 지급 가능하지만 중복 방지됨
        };
    }

    void Start()
    {
        if (txt_Dialogue == null)
        {
            Debug.LogError("[passenger_text_TMP] TMP_Text가 비어있습니다.");
            enabled = false; return;
        }
        if (typewriter == null)
        {
            typewriter = txt_Dialogue.GetComponent<TypewriterEffect>() ?? GetComponent<TypewriterEffect>();
        }
        ShowCurrent();
        TryGiveCluesForLine(index); // 첫 줄에서 바로 단서 지급
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey))
        {
            if (typewriter != null && typewriter.IsTyping) { typewriter.Skip(lines[index]); return; }
            Next();
        }
    }

    void ShowCurrent()
    {
        if (typewriter != null) typewriter.Play(lines[index]);
        else txt_Dialogue.text = lines[index];
    }

    void Next()
    {
        index = Mathf.Clamp(index + 1, 0, lines.Length - 1);
        ShowCurrent();
        TryGiveCluesForLine(index);
    }

    void TryGiveCluesForLine(int lineIdx)
    {
        if (!clueByLine.TryGetValue(lineIdx, out var clues)) return;
        foreach (var c in clues)
        {
            if (_given.Contains(c)) continue;
            ClueManager.I?.Add(c);
            _given.Add(c);
        }
    }
}
