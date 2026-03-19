// Quest용 Element 인터랙션 브릿지 스크립트
// XR Interaction Toolkit 이벤트를 Element 메서드에 연결합니다
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HoloToolkit.MRDL.PeriodicTable
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class QuestElementInteraction : MonoBehaviour
    {
        [Tooltip("닫기(X) 버튼으로 사용할 경우 체크. selectEntered 시 ResetActiveElement를 호출합니다.")]
        public bool isCloseButton = false;

        private XRSimpleInteractable interactable;
        private Element element;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            element = GetComponent<Element>();
            // 닫기 버튼은 Element 컴포넌트가 없으므로 부모 계층에서 탐색
            if (element == null)
                element = GetComponentInParent<Element>();
        }

        void OnEnable()
        {
            if (interactable == null) return;
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.activated.AddListener(OnActivated);
            if (!isCloseButton)
            {
                interactable.hoverEntered.AddListener(OnHoverEntered);
                interactable.hoverExited.AddListener(OnHoverExited);
            }
        }

        void OnDisable()
        {
            if (interactable == null) return;
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.activated.RemoveListener(OnActivated);
            if (!isCloseButton)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEntered);
                interactable.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (element == null) return;
            if (isCloseButton)
                element.ResetActiveElement();
            else
            {
                element.SetActiveElement();
                element.Open();
            }
        }

        private void OnActivated(ActivateEventArgs args)
        {
            if (element == null) return;
            if (isCloseButton)
                element.ResetActiveElement();
            else
            {
                element.SetActiveElement();
                element.Open();
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            element?.Highlight();
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            element?.Dim();
        }
    }
}
