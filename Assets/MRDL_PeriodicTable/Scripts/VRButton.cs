// OVR 컨트롤러 레이캐스트로 클릭 가능한 버튼 컴포넌트
// OVRElementInteractor가 Physics.Raycast로 이 컴포넌트를 감지하여 onClick을 호출합니다.
using UnityEngine;
using UnityEngine.Events;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class VRButton : MonoBehaviour
    {
        public UnityEvent onClick = new UnityEvent();

        private Vector3 originalScale;

        void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnSelect()
        {
            onClick?.Invoke();
        }

        public void OnHoverEnter()
        {
            transform.localScale = originalScale * 1.1f;
        }

        public void OnHoverExit()
        {
            transform.localScale = originalScale;
        }
    }
}
