using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleCloudTTS : MonoBehaviour
{
    [Header("Google Cloud API")]
    [Tooltip("개발 단계에서만 사용. 출시 전에는 백엔드 프록시를 권장!")]
    public string apiKey; // GCP Text-to-Speech API 키

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Default Voice Settings")]
    public string languageCode = "ko-KR";       
    public string voiceName = "ko-KR-Wavenet-B"; // 목소리 종류
    [Range(0.25f, 4f)] public float speakingRate = 1.0f;
    [Range(-20f, 20f)] public float pitch = 0f;

    [Serializable] class TTSInput { public string text; /* SSML 쓸 거면 public string ssml; 추가 */ }

    // 성별 필드(ssmlGender) 제거해서 보이스 이름만 사용 (보이스가 성별을 내포)
    [Serializable] class TTSVoice { public string languageCode; public string name; }

    [Serializable]
    class TTSConfig
    {
        public string audioEncoding;  // "MP3", "OGG_OPUS", "LINEAR16"
        public float speakingRate;
        public float pitch;
    }

    [Serializable]
    class TTSRequest
    {
        public TTSInput input;
        public TTSVoice voice;
        public TTSConfig audioConfig;
    }

    [Serializable] class TTSResponse { public string audioContent; }

    const string Endpoint = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";

    /// <summary>
    /// 간편 호출: 현재 설정으로 문장을 읽어줌
    /// </summary>
    public void Speak(string text)
    {
        StartCoroutine(CoSpeak(text, languageCode, voiceName, speakingRate, pitch));
    }

    /// <summary>
    /// 필요 시 런타임에 매개변수 바꿔서 호출
    /// </summary>
    public IEnumerator CoSpeak(string text, string lang, string vName, float rate, float pitchVal)
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("GoogleCloudTTS: API Key가 비어있습니다.");
            yield break;
        }
        if (string.IsNullOrWhiteSpace(text)) yield break;

        var req = new TTSRequest
        {
            input = new TTSInput { text = text },
            voice = new TTSVoice { languageCode = lang, name = vName },
            audioConfig = new TTSConfig
            {
                audioEncoding = "MP3",
                speakingRate = rate,
                pitch = pitchVal
            }
        };

        string json = JsonUtility.ToJson(req);
        string url = Endpoint + apiKey;

        using (var uwr = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            yield return uwr.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (uwr.result != UnityWebRequest.Result.Success)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError($"TTS 요청 실패: {uwr.error}\n{uwr.downloadHandler.text}");
                yield break;
            }

            var res = JsonUtility.FromJson<TTSResponse>(uwr.downloadHandler.text);
            if (string.IsNullOrEmpty(res.audioContent))
            {
                Debug.LogError("GoogleCloudTTS: audioContent가 비어 있습니다.");
                yield break;
            }

            // base64 → mp3 바이트
            byte[] mp3Data = Convert.FromBase64String(res.audioContent);

            // 임시 파일로 저장 후 로드(안정적)
            string tempPath = Path.Combine(Application.temporaryCachePath, $"tts_{DateTime.Now.Ticks}.mp3");
            File.WriteAllBytes(tempPath, mp3Data);

            using (var audioReq = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
            {
                yield return audioReq.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (audioReq.result != UnityWebRequest.Result.Success)
#else
                if (audioReq.isNetworkError || audioReq.isHttpError)
#endif
                {
                    Debug.LogError($"오디오 로드 실패: {audioReq.error}");
                    yield break;
                }

                var clip = DownloadHandlerAudioClip.GetContent(audioReq);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
}
