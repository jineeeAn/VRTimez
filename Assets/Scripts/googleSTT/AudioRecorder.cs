using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AudioRecorder : MonoBehaviour
{
    AudioClip clip;
    public string fileName = "recorded.wav";

    public void StartRecording()
    {
        // 16kHz, 모노
        clip = Microphone.Start(null, false, 5, 16000);
        Debug.Log("녹음 시작");
    }

    public string StopAndSave()
    {
        Microphone.End(null);
        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, SavWav.GetWavBytes(clip, trim: true));
        Debug.Log("저장됨: " + path);
        return path;
    }
}
