using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLMUnitySamples;

public class STTrunner : MonoBehaviour
{
    public AudioRecorder recorder;
    public googleSTT googleStt;
    public ChatBot chatBot; 
    public float recordDuration = 5f;

    public void OnClick_RecordAndSend()
    {
        StartCoroutine(CoRecordAndSend());
    }

    IEnumerator CoRecordAndSend()
    {
        recorder.StartRecording();
        yield return new WaitForSeconds(recordDuration);

        string wav = recorder.StopAndSave();
        if (string.IsNullOrEmpty(wav)) yield break;

        yield return StartCoroutine(googleStt.Recognize(wav));

        // STT °á°ú¸¦ ¹Ù·Î Ãªº¿¿¡ Àü¼Û
        chatBot.SendMessageFromExternal(googleStt.lastResult);
    }
}
