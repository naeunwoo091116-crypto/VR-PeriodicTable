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
        private XRSimpleInteractable interactable;
        private Element element;

        void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            element = GetComponent<Element>();
        }

        void OnEnable()
        {
            if (interactable == null) return;
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.activated.AddListener(OnActivated);
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
        }

        void OnDisable()
        {
            if (interactable == null) return;
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.activated.RemoveListener(OnActivated);
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (element == null) return;
            element.SetActiveElement();
            element.Open();
        }

        private void OnActivated(ActivateEventArgs args)
        {
            if (element == null) return;
            element.SetActiveElement();
            element.Open();
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
