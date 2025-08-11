using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    public float typingSpeed = 0.03f;

    private TMP_Text textComponent;
    private Coroutine typingRoutine;
    private bool isTyping;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    /// <summary>외부에서 문자열을 넘겨주면 타자 효과로 출력</summary>
    public void Play(string fullText)
    {
        // 이미 타이핑 중이면 중단
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeText(fullText));
    }

    /// <summary>즉시 전체 출력(스킵)</summary>
    public void Skip(string fullText)
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        textComponent.text = fullText;
        isTyping = false;
        typingRoutine = null;
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        textComponent.text = "";
        foreach (char letter in fullText)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        typingRoutine = null;
    }

    public bool IsTyping => isTyping;
}
