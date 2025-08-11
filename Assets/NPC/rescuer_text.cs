using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Clue_Id;

public class rescuer_text : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text txt_Dialogue;
    public TypewriterEffect typewriter;   // 비워두면 자동탐색

    [Header("Input")]
    public KeyCode nextKey = KeyCode.G;

    // 대사
    private static string[] lines;
    private int index = 0;

    // 줄별로 지급할 단서들(여러 개 가능)
    private Dictionary<int, ClueId[]> clueByLine;
    private HashSet<ClueId> given = new(); // 이 NPC에서 이미 준 단서

    void Awake()
    {
        lines = new string[] {
            "\n\n가방 하나가 저기엔 있었는데… 지금 없어졌어요.",                   // 0
            "\n\n누가 치운 건지, 불이 나기 전부터 거기에 있던 걸 분명 봤습니다.",     // 1
            "\n\n좌석 아래에서 라이터 뚜껑 같은 걸 주웠습니다.",                    // 2
            "\n\n그리고 바닥에 기름 냄새가 강하게 남아 있어요. 누군가 흘린 것 같습니다.", // 3
            "\n\nCCTV를 보면, 가방을 누가 가져갔는지 확인할 수 있을 겁니다."          // 4 (정보 가이드)
        };

        clueByLine = new Dictionary<int, ClueId[]> {
            { 0, new[]{ ClueId.Rescuer_BagMissing } },
            { 2, new[]{ ClueId.Rescuer_Bag_Lighter } },
            { 3, new[]{ ClueId.Rescuer_Bag_FuelSmell } }
        };
    }

    void Start()
    {
        if (txt_Dialogue == null)
        {
            Debug.LogError("[rescuer_text_TMP] TMP_Text가 비어있습니다.");
            enabled = false; return;
        }
        if (typewriter == null)
        {
            typewriter = txt_Dialogue.GetComponent<TypewriterEffect>() ?? GetComponent<TypewriterEffect>();
        }
        ShowCurrent();
        TryGiveCluesForLine(index); // 첫 줄도 바로 단서 지급
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
        if (clueByLine == null || !clueByLine.TryGetValue(lineIdx, out var clues)) return;
        foreach (var c in clues)
        {
            if (given.Contains(c)) continue;
            ClueManager.I?.Add(c);
            given.Add(c);
        }
    }
}
