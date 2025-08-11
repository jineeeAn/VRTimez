using System;
using System.Collections.Generic;
using UnityEngine;
using static Clue_Id;

public class ClueManager : MonoBehaviour
{
    public static ClueManager I { get; private set; }

    [Header("Persistence")]
    [Tooltip("플레이 종료 후에도 단서를 유지할지 여부 (기본: 해제)")]
    public bool persistBetweenRuns = false;

    // 이미 얻은 단서 Set
    private readonly HashSet<ClueId> got = new();

    // 새 단서가 추가될 때 알림
    public event Action<ClueId> OnClueAdded;

    // ▶ 플레이 "시작 직전"마다 실행: 에디터 Enter Play, 빌드 실행 모두 포함
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetAtBoot()
    {
        // persistBetweenRuns=false 인 경우를 대비해, 시작 전에 저장값을 지워버리면
        // Awake() -> Load() 호출 시 빈 상태로 시작함.
        PlayerPrefs.DeleteKey("clues");
        PlayerPrefs.Save();
    }

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        Load(); // 시작 시 저장된 단서 불러오기(옵션)
    }

    void OnApplicationQuit()
    {
        // 에디터에서 '중지' 눌렀을 때도 호출됨
        if (!persistBetweenRuns)
        {
            PlayerPrefs.DeleteKey("clues");
            PlayerPrefs.Save();
        }
        got.Clear();
        OnClueAdded?.Invoke(ClueId.None); // NotebookUI 갱신 트리거
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    public bool Has(ClueId id) => got.Contains(id);

    public void Add(ClueId id)
    {
        if (id == ClueId.None || got.Contains(id)) return;
        got.Add(id);
        Save();
        OnClueAdded?.Invoke(id);
        Debug.Log($"[Clue] Added: {id}");
    }

    public IEnumerable<ClueId> All() => got;

    // 👉 외부에서 강제로 전부 비우고 싶을 때(디버그/챕터 리셋 등)
    public void Clear()
    {
        got.Clear();
        Save(); // persistBetweenRuns=true면 공백으로 저장
        OnClueAdded?.Invoke(ClueId.None);
        Debug.Log("[Clue] Cleared all clues.");
    }

    void Save()
    {
        if (!persistBetweenRuns) return;           // 유지 안 할 거면 저장 스킵
        PlayerPrefs.SetString("clues", string.Join(",", got));
        PlayerPrefs.Save();
    }

    void Load()
    {
        got.Clear();
        if (!persistBetweenRuns) return;           // 유지 안 할 거면 로드 스킵(빈 상태)
        var s = PlayerPrefs.GetString("clues", "");
        if (string.IsNullOrEmpty(s)) return;
        foreach (var tok in s.Split(','))
        {
            if (Enum.TryParse(tok, out ClueId id)) got.Add(id);
        }
    }
}
