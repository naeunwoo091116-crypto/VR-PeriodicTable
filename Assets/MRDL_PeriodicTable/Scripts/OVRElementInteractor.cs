// OVR 컨트롤러용 주기율표 인터랙터
// 원소 클릭과 완전히 동일한 방식으로 레이아웃 버튼 / X버튼도 처리
// 원자 그랩: 레이가 Layer 9(Atoms)에 히트한 상태에서 트리거를 누르면 원자를 이동
using UnityEngine;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class OVRElementInteractor : MonoBehaviour
    {
        [Header("OVR 설정")]
        public OVRInput.Controller controller = OVRInput.Controller.RTouch;

        [Header("레이 설정")]
        public float maxRayDistance = 5f;
        public LayerMask raycastMask = -1;

        [Header("레이 색상")]
        public Color rayColorNormal = new Color(0f, 0.8f, 1f, 0.8f);
        public Color rayColorHover  = new Color(1f, 1f, 0f, 1f);
        public Color rayColorGrab   = new Color(0f, 1f, 0.3f, 1f);

        private LineRenderer line;
        private Element  hoveredElement;
        private VRButton hoveredButton;

        // 원자 그랩 상태
        private bool      isGrabbing          = false;
        private Transform grabbedAtomTransform = null;
        private float     grabDistance         = 0f;

        void Awake()
        {
            // 원소와 같은 레이어(Default=0, ElementBox=8, Atoms=9)
            raycastMask = (1 << 0) | (1 << 8) | (1 << 9);

            line = gameObject.GetComponent<LineRenderer>();
            if (line == null) line = gameObject.AddComponent<LineRenderer>();
            line.positionCount     = 2;
            line.startWidth        = 0.004f;
            line.endWidth          = 0.001f;
            line.useWorldSpace     = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows    = false;
            line.material          = new Material(Shader.Find("Sprites/Default"));
            SetLineColor(rayColorNormal);
        }

        void Start()
        {
            SetupLayoutButtons();
        }

        // ── 레이아웃 버튼 설정 ────────────────────────────────────────
        void SetupLayoutButtons()
        {
            var lsc = FindFirstObjectByType<LayoutStyleChanger>();
            if (lsc == null) return;

            Attach("ButtonPlane",    lsc.ChangeLayoutStylePlane);
            Attach("ButtonCylinder", lsc.ChangeLayoutStyleCylinder);
            Attach("ButtonSphere",   lsc.ChangeLayoutStyleSphere);
            Attach("ButtonRadial",   lsc.ChangeLayoutStyleRadial);
        }

        void Attach(string goName, System.Action action)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (t.gameObject.name != goName) continue;
                var go = t.gameObject;

                if (go.GetComponent<Collider>() == null)
                {
                    var bc  = go.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.032f, 0.032f, 0.012f);
                }

                var btn = go.GetComponent<VRButton>() ?? go.AddComponent<VRButton>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => action());
                Debug.Log($"[OVRElementInteractor] 버튼 준비: {go.name}");
            }
        }

        // ── 메인 루프 ─────────────────────────────────────────────────
        void Update()
        {
            Ray ray      = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            Element  hitElem   = null;
            VRButton hitButton = null;
            bool     hitAtom   = false;
            Vector3  endPoint  = transform.position + transform.forward * maxRayDistance;

            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastMask))
            {
                endPoint = hit.point;
                if (hit.collider.gameObject.layer == 9)
                {
                    // Layer 9 = Atoms: 그랩 전용, 원소 선택/닫기 대상에서 제외
                    hitAtom = true;
                }
                else
                {
                    hitElem = hit.collider.GetComponentInParent<Element>();
                    if (hitElem == null)
                        hitButton = hit.collider.GetComponentInParent<VRButton>();
                }
            }

            // ── 원소 닫힘 시 그랩 자동 해제 ──────────────────────────
            if (isGrabbing && Element.ActiveElement == null)
            {
                isGrabbing          = false;
                grabbedAtomTransform = null;
            }

            // ── 그랩 중 Atom 이동 ─────────────────────────────────────
            if (isGrabbing && grabbedAtomTransform != null)
            {
                grabbedAtomTransform.position =
                    transform.position + transform.forward * grabDistance;
            }

            // ── 트리거 놓음 → 그랩 해제 ──────────────────────────────
            if (isGrabbing && OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                isGrabbing          = false;
                grabbedAtomTransform = null;
            }

            // ── 레이 시각화 ──────────────────────────────────────────
            line.SetPosition(0, transform.position);
            line.SetPosition(1, isGrabbing ? transform.position + transform.forward * grabDistance : endPoint);

            // ── 호버 처리 ─────────────────────────────────────────────
            if (!isGrabbing)
            {
                if (hitElem != hoveredElement)
                {
                    hoveredElement?.Dim();
                    hoveredElement = hitElem;
                    hoveredElement?.Highlight();
                }
                if (hitButton != hoveredButton)
                {
                    hoveredButton?.OnHoverExit();
                    hoveredButton = hitButton;
                    hoveredButton?.OnHoverEnter();
                }
            }

            if (isGrabbing)
                SetLineColor(rayColorGrab);
            else if (hoveredElement != null || hoveredButton != null || hitAtom)
                SetLineColor(rayColorHover);
            else
                SetLineColor(rayColorNormal);

            // ── 트리거 누름 ───────────────────────────────────────────
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                if (hitAtom && Element.ActiveElement != null)
                {
                    // Atom 그랩 시작
                    isGrabbing           = true;
                    grabbedAtomTransform = hit.collider.transform;
                    grabDistance         = hit.distance;
                }
                else if (hoveredElement != null)
                {
                    // 이미 열린 원소 → 닫기, 아니면 열기
                    if (Element.ActiveElement == hoveredElement)
                        hoveredElement.ResetActiveElement();
                    else
                    {
                        hoveredElement.SetActiveElement();
                        hoveredElement.Open();
                    }
                }
                else if (hoveredButton != null)
                {
                    hoveredButton.OnSelect();
                }
            }
        }

        void OnDisable()
        {
            hoveredElement?.Dim();        hoveredElement       = null;
            hoveredButton?.OnHoverExit(); hoveredButton        = null;
            isGrabbing          = false;
            grabbedAtomTransform = null;
            if (line != null) line.enabled = false;
        }

        void OnEnable()  { if (line != null) line.enabled = true; }

        void SetLineColor(Color c)
        {
            if (line == null) return;
            line.startColor = c;
            line.endColor   = new Color(c.r, c.g, c.b, c.a * 0.3f);
        }
    }
}
