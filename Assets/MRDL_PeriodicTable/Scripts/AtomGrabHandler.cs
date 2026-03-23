using UnityEngine;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// 원소가 열렸을 때 원자 모델을 OVR 컨트롤러로 잡을 수 있게 해주는 컴포넌트.
    /// - 원소 열림: Element의 BoxCollider 비활성화, Atom SphereCollider 활성화
    ///   → 레이캐스트가 Element Box 대신 Atom을 우선 감지하게 됨
    /// - 원소 닫힘: 반대로 복구
    /// </summary>
    [RequireComponent(typeof(Element))]
    public class AtomGrabHandler : MonoBehaviour
    {
        private Element element;
        private BoxCollider elementBoxCollider;
        private SphereCollider grabCollider;
        private Vector3 atomOriginalLocalPosition;
        private Quaternion atomOriginalLocalRotation;
        private bool wasActive = false;

        private void Start()
        {
            element = GetComponent<Element>();
            SetupCollider();
        }

        private void SetupCollider()
        {
            GameObject atomObj = element.Atom.gameObject;

            // 원자의 초기 로컬 위치/회전 저장 (원소 닫힐 때 복구용)
            atomOriginalLocalPosition = atomObj.transform.localPosition;
            atomOriginalLocalRotation = atomObj.transform.localRotation;

            // Atom GameObject의 레이어를 9(Atoms)로 설정
            atomObj.layer = 9;

            // 레이캐스트 감지용 SphereCollider (비활성 상태로 시작)
            grabCollider = atomObj.GetComponent<SphereCollider>();
            if (grabCollider == null)
                grabCollider = atomObj.AddComponent<SphereCollider>();
            grabCollider.radius = 0.15f;
            grabCollider.isTrigger = false;
            grabCollider.enabled = false;

            // Element 자체의 BoxCollider 참조 (열릴 때 비활성화해야 함)
            // Physics.Raycast는 첫 번째 히트만 반환하므로 BoxCollider가 Atom보다
            // 먼저 히트되면 원소 닫힘 로직이 실행되어버림
            elementBoxCollider = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            bool isActive = Element.ActiveElement == element;

            if (isActive && !wasActive)
            {
                // 원소가 열림
                // 1) Element BoxCollider 비활성화 → 레이가 Atom만 감지
                if (elementBoxCollider != null)
                    elementBoxCollider.enabled = false;
                // 2) Atom SphereCollider 활성화
                grabCollider.enabled = true;
            }
            else if (!isActive && wasActive)
            {
                // 원소가 닫힘
                // 1) Atom SphereCollider 비활성화
                grabCollider.enabled = false;
                // 2) Element BoxCollider 복구
                if (elementBoxCollider != null)
                    elementBoxCollider.enabled = true;
                // 3) 원자 위치 초기화
                element.Atom.transform.localPosition = atomOriginalLocalPosition;
                element.Atom.transform.localRotation = atomOriginalLocalRotation;
            }

            wasActive = isActive;
        }
    }
}
