// NotebookUI.cs
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;
using static Clue_Id;

public class NotebookUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text cluesText;

    void OnEnable()
    {
        TryHook();
        Apply();
    }

    void OnDisable()
    {
        if (ClueManager.I != null)
            ClueManager.I.OnClueAdded -= OnClueAdded;
    }

    void TryHook()
    {
        if (ClueManager.I != null)
        {
            ClueManager.I.OnClueAdded -= OnClueAdded; // 중복구독 방지
            ClueManager.I.OnClueAdded += OnClueAdded;
        }
        else
        {
            // ClueManager가 나중에 생성될 수도 있으니 대기
            StartCoroutine(WaitForClueManager());
        }
    }

    IEnumerator WaitForClueManager()
    {
        while (ClueManager.I == null) yield return null;
        TryHook();
        Apply();
    }

    void OnClueAdded(ClueId _) => Apply();

    void Apply()
    {
        if (!cluesText) return;
        if (ClueManager.I == null)
        {
            cluesText.text = "획득한 단서 없음";
            return;
        }

        var sb = new StringBuilder();
        foreach (var c in ClueManager.I.All())
            sb.AppendLine(Pretty(c));

        cluesText.text = sb.Length == 0 ? "획득한 단서 없음" : sb.ToString();
    }

    string Pretty(ClueId id) => id switch
    {
        ClueId.Boy_EmergencyDoor_BeforeFire => "남자어린이: 불 나기 전부터 비상문 쪽 이동 목격",
        ClueId.Rescuer_BagMissing => "구조대원: 현장 가방 1개 사라짐(증거 인멸 정황)",
        ClueId.Rescuer_Bag_Lighter => "구조대원: 라이터 부품(뚜껑) 발견",
        ClueId.Rescuer_Bag_FuelSmell => "구조대원: 좌석/바닥에 기름 냄새 흔적",
        ClueId.Passenger_WatchMemo => "침착한 승객: 시계를 보며 메모 (계획 정황)",
        ClueId.Passenger_TimingHint => "침착한 승객: 특정 시간/타이밍을 염두에 둠",
        _ => id.ToString()
    };
}
