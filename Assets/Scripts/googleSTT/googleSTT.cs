using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public class googleSTT : MonoBehaviour
{
    public string apiKey; // 구글 콘솔에서 발급받은 API 키
    [TextArea] public string lastResult;

    public IEnumerator Recognize(string wavPath)
    {
        byte[] audio = File.ReadAllBytes(wavPath);
        string content = Convert.ToBase64String(audio);

        var requestJson = $@"{{
  ""config"": {{
    ""encoding"": ""LINEAR16"",
    ""sampleRateHertz"": 16000,
    ""languageCode"": ""ko-KR""
  }},
  ""audio"": {{
    ""content"": ""{content}""
  }}
}}";

        string url = $"https://speech.googleapis.com/v1/speech:recognize?key={apiKey}";
        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(req.error + "\n" + req.downloadHandler.text);
                yield break;
            }

            string json = req.downloadHandler.text;
            string transcript = ExtractTranscript(json);
            lastResult = transcript;
            Debug.Log("STT: " + transcript);
        }
    }

    string ExtractTranscript(string json)
    {
        var key = "\"transcript\":";
        int i = json.IndexOf(key);
        if (i < 0) return "(no transcript)";
        int s = json.IndexOf('"', i + key.Length);
        int e = json.IndexOf('"', s + 1);
        return json.Substring(s + 1, e - (s + 1));
    }
}
