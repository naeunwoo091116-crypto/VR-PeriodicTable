// Quest 원소/닫기 버튼 인터랙션 스크립트
// OVRElementInteractor의 Physics.Raycast로 감지되는 VRButton을 자동으로 추가합니다.
using UnityEngine;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class QuestElementInteraction : MonoBehaviour
    {
        [Tooltip("닫기(X) 버튼으로 사용할 경우 체크. 클릭 시 ResetActiveElement를 호출합니다.")]
        public bool isCloseButton = false;

        private Element element;

        void Awake()
        {
            element = GetComponent<Element>();
            if (element == null)
                element = GetComponentInParent<Element>();

            // Collider 없으면 추가 (Raycast 필요)
            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<BoxCollider>();
                col.size   = new Vector3(0.032f, 0.032f, 0.012f);
                col.center = Vector3.zero;
            }

            // VRButton 추가 및 onClick 연결
            var btn = GetComponent<VRButton>();
            if (btn == null)
                btn = gameObject.AddComponent<VRButton>();

            btn.onClick.RemoveAllListeners();
            if (isCloseButton)
                btn.onClick.AddListener(OnCloseClicked);
            else
                btn.onClick.AddListener(OnElementClicked);
        }

        private void OnElementClicked()
        {
            if (element == null) return;
            element.SetActiveElement();
            element.Open();
        }

        private void OnCloseClicked()
        {
            if (element == null) return;
            element.ResetActiveElement();
        }
    }
}
