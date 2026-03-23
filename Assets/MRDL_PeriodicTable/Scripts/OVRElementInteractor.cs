// OVR 컨트롤러용 주기율표 인터랙터
// 원소 클릭과 완전히 동일한 방식으로 레이아웃 버튼 / X버튼도 처리
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

        private LineRenderer line;
        private Element  hoveredElement;
        private VRButton hoveredButton;

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
        // 원소가 Element 컴포넌트 + BoxCollider를 갖는 것처럼
        // 버튼도 VRButton 컴포넌트 + BoxCollider를 갖도록 설정
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

                // 원소처럼 Collider 확보
                if (go.GetComponent<Collider>() == null)
                {
                    var bc  = go.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.032f, 0.032f, 0.012f);
                }

                // 원소처럼 클릭 컴포넌트 부착
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
            Vector3  endPoint  = transform.position + transform.forward * maxRayDistance;

            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastMask))
            {
                endPoint  = hit.point;
                // Layer 9(Atoms)는 원자 그랩 전용 — 원소 선택/닫기 대상에서 제외
                if (hit.collider.gameObject.layer != 9)
                {
                    hitElem   = hit.collider.GetComponentInParent<Element>();
                    if (hitElem == null)
                        hitButton = hit.collider.GetComponentInParent<VRButton>();
                }
            }

            line.SetPosition(0, transform.position);
            line.SetPosition(1, endPoint);

            // 호버 처리
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
            SetLineColor((hoveredElement != null || hoveredButton != null) ? rayColorHover : rayColorNormal);

            // 트리거 — 원소와 완전히 동일한 OVRInput.GetDown 방식
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                if (hoveredElement != null)
                {
                    // 이미 열린 원소 → 닫기 (X버튼 역할)
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
                    // 레이아웃 버튼 — 원소 Open()과 동일한 흐름
                    hoveredButton.OnSelect();
                }
            }
        }

        void OnDisable()
        {
            hoveredElement?.Dim();   hoveredElement = null;
            hoveredButton?.OnHoverExit(); hoveredButton = null;
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
