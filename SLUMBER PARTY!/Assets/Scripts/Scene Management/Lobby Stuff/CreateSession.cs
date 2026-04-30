using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;


namespace SLUMBER_PARTY.LobbyUtils
{
    public class CreateSession : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_inputField;
        public TextMeshProUGUI m_codeText;

        public void Awake()
        {
            m_inputField = GetComponentInChildren<TMP_InputField>();

            m_inputField.onEndEdit.AddListener(value =>
            {
                if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(value))
                {
                    TestSessionManager.Instance.CreateSession(value);
                }
            });
        }

        public void Update()
        {
            if (TestSessionManager.Instance == null)
                return;

            if (TestSessionManager.Instance.GetJoinedSession() != null)
            {
                UpdateCodeText(TestSessionManager.Instance.GetJoinedSession().Code);
                return;
            }
        }

        public void UpdateCodeText(string code)
        {
            StartCoroutine(GetCode(0.8f, code));
        }

        IEnumerator GetCode(float time, string code)
        {
            yield return new WaitForSeconds(time);

            m_codeText.SetText(code);
        }
    }
}

