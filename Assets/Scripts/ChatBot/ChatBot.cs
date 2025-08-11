using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.UI;
using System; // Action 쓰려고


namespace LLMUnitySamples
{
    public class ChatBot : MonoBehaviour
    {
        public Transform chatContainer;
        public Color playerColor = new Color32(81, 164, 81, 255);
        public Color aiColor = new Color32(29, 29, 73, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public LLMCharacter llmCharacter;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;

        private InputBubble inputBubble;
        private List<Bubble> chatBubbles = new List<Bubble>();
        private bool blockInput = true;
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;

        public GoogleCloudTTS tts; //tts 할당하기!
        
        // STT 결과를 바로 전송하는 public 메서드
        public void SendMessageFromExternal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // 입력창에 텍스트 넣기
            inputBubble.SetText(text.Replace("\v", "\n"));

            // blockInput이 true면 입력 허용
            if (blockInput) AllowInput();

            // 기존 입력 처리 로직 호출
            onInputFieldSubmit(text);
        }


        



        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            inputBubble.AddSubmitListener(onInputFieldSubmit);
            inputBubble.AddValueChangedListener(onValueChanged);
            inputBubble.setInteractable(false);
            stopButton.gameObject.SetActive(true);
            ShowLoadedMessages();
            _ = llmCharacter.Warmup(WarmUpCallback);
        }

        Bubble AddBubble(string message, bool isPlayerMessage)
        {
            Bubble bubble = new Bubble(chatContainer, isPlayerMessage? playerUI: aiUI, isPlayerMessage? "PlayerBubble": "AIBubble", message);
            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        void ShowLoadedMessages()
        {
            for (int i=1; i<llmCharacter.chat.Count; i++) AddBubble(llmCharacter.chat[i].content, i%2==1);
        }

        void onInputFieldSubmit(string newText)
        {
            inputBubble.ActivateInputField();
            if (blockInput || newText.Trim() == "" || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                StartCoroutine(BlockInteraction());
                return;
            }
            blockInput = true;

            // replace vertical_tab
            string message = inputBubble.GetText().Replace("\v", "\n");

            AddBubble(message, true);
            Bubble aiBubble = AddBubble("...", false);

            // 최종 텍스트를 담아둘 버퍼
            string finalAIText = "";

            // 1) 델타 콜백: 화면만 갱신 + 최종 문자열 누적/갱신
            LLMUnity.Callback<string> onDelta = (partial) =>
            {
                finalAIText = partial;      // LLM 샘플은 partial에 '현재까지의 전체'가 들어옴
                aiBubble.SetText(partial);  // UI 업데이트만
            };

            // 2) 완료 콜백: 여기서 한 번만 읽기
            LLMUnity.EmptyCallback onDone = () =>
            {
                AllowInput();

                // 깨끗하게 정리
                var speakText = (finalAIText ?? "").Replace("\v", "\n").Trim();
                if (!string.IsNullOrEmpty(speakText) && tts != null)
                {
                    tts.Speak(speakText);   // ★ 끝났을 때 한 번만 재생
                }
            };

            // Chat 호출
            Task chatTask = llmCharacter.Chat(message, onDelta, onDone);

            inputBubble.SetText("");
        }






        public void WarmUpCallback()
        {
            warmUpDone = true;
            inputBubble.SetPlaceHolderText("Ask Hampson anything");
            AllowInput();
        }

        public void AllowInput()
        {
            blockInput = false;
            inputBubble.ReActivateInputField();
        }

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            AllowInput();
        }

        IEnumerator<string> BlockInteraction()
        {
            // prevent from change until next frame
            inputBubble.setInteractable(false);
            yield return null;
            inputBubble.setInteractable(true);
            // change the caret position to the end of the text
            inputBubble.MoveTextEnd();
        }

        void onValueChanged(string newText)
        {
            // Get rid of newline character added when we press enter
            if (Input.GetKey(KeyCode.Return))
            {
                if (inputBubble.GetText().Trim() == "")
                    inputBubble.SetText("");
            }
        }

        public void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;
            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                Bubble bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition = new Vector2(childRect.anchoredPosition.x, y);

                // last bubble outside the container
                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                {
                    lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        void Update()
        {
            if (!inputBubble.inputFocused() && warmUpDone)
            {
                inputBubble.ActivateInputField();
                StartCoroutine(BlockInteraction());
            }
            if (lastBubbleOutsideFOV != -1)
            {
                // destroy bubbles outside the container
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                {
                    chatBubbles[i].Destroy();
                }
                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }

        bool onValidateWarning = true;
        void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
    }
}
