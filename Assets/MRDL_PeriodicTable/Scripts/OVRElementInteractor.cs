// OVR 컨트롤러용 주기율표 원소 인터랙터
// XR Origin 없이 Meta Building Block 컨트롤러에서 직접 작동
using UnityEngine;
using HoloToolkit.MRDL.PeriodicTable;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class OVRElementInteractor : MonoBehaviour
    {
        [Header("OVR 설정")]
        [Tooltip("RTouch = 오른손, LTouch = 왼손")]
        public OVRInput.Controller controller = OVRInput.Controller.RTouch;

        [Header("레이 설정")]
        public float maxRayDistance = 5f;
        public LayerMask raycastMask = -1;

        [Header("레이 색상")]
        public Color rayColorNormal = new Color(0f, 0.8f, 1f, 0.8f);
        public Color rayColorHover  = new Color(1f, 1f, 0f, 1f);

        private LineRenderer line;
        private Element hoveredElement;

        void Awake()
        {
            // LayerMask: ElementBox(8) + Atoms(9) + Default(0)
            raycastMask = (1 << 0) | (1 << 8) | (1 << 9);

            // LineRenderer 자동 생성
            line = gameObject.GetComponent<LineRenderer>();
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            line.positionCount    = 2;
            line.startWidth       = 0.004f;
            line.endWidth         = 0.001f;
            line.useWorldSpace    = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows   = false;

            var mat = new Material(Shader.Find("Sprites/Default"));
            line.material = mat;
            SetLineColor(rayColorNormal);
        }

        void Update()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            Element hitElem = null;
            Vector3 endPoint = transform.position + transform.forward * maxRayDistance;

            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastMask))
            {
                endPoint = hit.point;
                hitElem  = hit.collider.GetComponentInParent<Element>();
            }

            // 라인 렌더러 업데이트
            line.SetPosition(0, transform.position);
            line.SetPosition(1, endPoint);

            // 호버 상태 변경
            if (hitElem != hoveredElement)
            {
                if (hoveredElement != null)
                    hoveredElement.Dim();

                hoveredElement = hitElem;

                if (hoveredElement != null)
                    hoveredElement.Highlight();

                SetLineColor(hoveredElement != null ? rayColorHover : rayColorNormal);
            }

            // 트리거 누름 → 원소 열기
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                if (hoveredElement != null)
                {
                    hoveredElement.SetActiveElement();
                    hoveredElement.Open();
                }
            }

            // (트리거 떼기 이벤트 - 필요시 추가)
        }

        void OnDisable()
        {
            if (hoveredElement != null)
            {
                hoveredElement.Dim();
                hoveredElement = null;
            }
            if (line != null)
                line.enabled = false;
        }

        void OnEnable()
        {
            if (line != null)
                line.enabled = true;
        }

        void SetLineColor(Color c)
        {
            if (line != null)
            {
                line.startColor = c;
                line.endColor   = new Color(c.r, c.g, c.b, c.a * 0.3f);
            }
        }
    }
}
