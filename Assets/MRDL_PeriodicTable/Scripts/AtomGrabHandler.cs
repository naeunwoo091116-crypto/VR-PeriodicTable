using UnityEngine;


namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// 원소가 열렸을 때 원자 모델을 XR 컨트롤러로 잡을 수 있게 해주는 컴포넌트.
    /// Element 컴포넌트와 같은 GameObject에 부착하면 자동으로 동작합니다.
    /// </summary>
    [RequireComponent(typeof(Element))]
    public class AtomGrabHandler : MonoBehaviour
    {
        private Element element;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
        private SphereCollider grabCollider;
        private Vector3 atomOriginalLocalPosition;
        private Quaternion atomOriginalLocalRotation;
        private bool wasActive = false;

        private void Start()
        {
            element = GetComponent<Element>();
            SetupGrabComponents();
        }

        private void SetupGrabComponents()
        {
            GameObject atomObj = element.Atom.gameObject;

            // 원자의 초기 로컬 위치/회전 저장 (원소 닫힐 때 복구용)
            atomOriginalLocalPosition = atomObj.transform.localPosition;
            atomOriginalLocalRotation = atomObj.transform.localRotation;

            // XRGrabInteractable에 필요한 Rigidbody 추가
            Rigidbody rb = atomObj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = atomObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // 그랩 감지용 SphereCollider (비활성 상태로 시작)
            grabCollider = atomObj.GetComponent<SphereCollider>();
            if (grabCollider == null)
                grabCollider = atomObj.AddComponent<SphereCollider>();
            grabCollider.radius = 0.15f;
            grabCollider.isTrigger = false;
            grabCollider.enabled = false;

            // XRGrabInteractable 추가 (비활성 상태로 시작)
            grabInteractable = atomObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable == null)
                grabInteractable = atomObj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable.MovementType.Kinematic;
            grabInteractable.throwOnDetach = false;
            grabInteractable.trackRotation = true;
            grabInteractable.enabled = false;
        }

        private void Update()
        {
            bool isActive = Element.ActiveElement == element;

            if (isActive && !wasActive)
            {
                // 원소가 열림 → 그랩 활성화
                grabCollider.enabled = true;
                grabInteractable.enabled = true;
            }
            else if (!isActive && wasActive)
            {
                // 원소가 닫힘 → 그랩 비활성화 및 원자 위치 초기화
                grabInteractable.enabled = false;
                grabCollider.enabled = false;
                element.Atom.transform.localPosition = atomOriginalLocalPosition;
                element.Atom.transform.localRotation = atomOriginalLocalRotation;
            }

            wasActive = isActive;
        }
    }
}
