// OVR 컨트롤러 트래킹 진단 스크립트
// 아무 빈 GameObject에 붙이고 Play → Console 확인
using UnityEngine;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class OVRControllerDiagnostic : MonoBehaviour
    {
        [Header("진단 설정")]
        [Tooltip("몇 프레임마다 로그 출력 (0 = 매 프레임)")]
        public int logInterval = 30; // 30프레임(약 0.5초)마다 출력

        private int frameCount = 0;
        private Vector3 lastRightPos = Vector3.zero;
        private Vector3 lastLeftPos  = Vector3.zero;
        private bool rightEverMoved  = false;
        private bool leftEverMoved   = false;

        void Start()
        {
            Debug.Log("=== [OVRDiag] 진단 시작 ===");
            Debug.Log($"[OVRDiag] OVRPlugin 버전: {OVRPlugin.version}");
            Debug.Log($"[OVRDiag] 연결된 컨트롤러: {OVRInput.GetConnectedControllers()}");
            Debug.Log($"[OVRDiag] 활성 컨트롤러: {OVRInput.GetActiveController()}");

            // OVRCameraRig 앵커 위치 확인
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null)
            {
                Debug.Log($"[OVRDiag] OVRCameraRig 월드위치: {rig.transform.position}");
                if (rig.rightHandAnchor != null)
                    Debug.Log($"[OVRDiag] RightHandAnchor 월드위치: {rig.rightHandAnchor.position} / 로컬: {rig.rightHandAnchor.localPosition}");
                if (rig.leftHandAnchor != null)
                    Debug.Log($"[OVRDiag] LeftHandAnchor 월드위치: {rig.leftHandAnchor.position} / 로컬: {rig.leftHandAnchor.localPosition}");
            }
            else
            {
                Debug.LogWarning("[OVRDiag] OVRCameraRig를 씬에서 찾을 수 없습니다!");
            }
        }

        void Update()
        {
            frameCount++;

            // OVRInput에서 직접 포즈 읽기
            Vector3 rightPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Vector3 leftPos  = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            Quaternion rightRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Quaternion leftRot  = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);

            // 움직임 감지
            if (Vector3.Distance(rightPos, lastRightPos) > 0.001f)
            {
                if (!rightEverMoved)
                {
                    rightEverMoved = true;
                    Debug.Log($"[OVRDiag] ✓ 오른쪽 컨트롤러 첫 이동 감지! 위치: {rightPos}");
                }
                lastRightPos = rightPos;
            }
            if (Vector3.Distance(leftPos, lastLeftPos) > 0.001f)
            {
                if (!leftEverMoved)
                {
                    leftEverMoved = true;
                    Debug.Log($"[OVRDiag] ✓ 왼쪽 컨트롤러 첫 이동 감지! 위치: {leftPos}");
                }
                lastLeftPos = leftPos;
            }

            // 주기적 로그
            if (logInterval <= 0 || frameCount % logInterval == 0)
            {
                Debug.Log($"[OVRDiag F{frameCount}] " +
                          $"Right로컬={rightPos:F3} rot={rightRot.eulerAngles:F1} | " +
                          $"Left로컬={leftPos:F3} rot={leftRot.eulerAngles:F1}");

                Debug.Log($"[OVRDiag F{frameCount}] " +
                          $"연결={OVRInput.GetConnectedControllers()} | " +
                          $"활성={OVRInput.GetActiveController()} | " +
                          $"Right이동여부={rightEverMoved} | Left이동여부={leftEverMoved}");

                // OVRCameraRig 앵커 현재 로컬 위치 (실제 적용값)
                var rig = FindFirstObjectByType<OVRCameraRig>();
                if (rig != null && rig.rightHandAnchor != null)
                {
                    Debug.Log($"[OVRDiag F{frameCount}] " +
                              $"RightHandAnchor 로컬={rig.rightHandAnchor.localPosition:F3} " +
                              $"월드={rig.rightHandAnchor.position:F3}");
                }
            }
        }

        void OnGUI()
        {
            // 화면에도 표시 (Console 열지 않아도 확인 가능)
            Vector3 r = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Vector3 l = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 18;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;

            string info =
                $"=== OVR 컨트롤러 진단 ===\n" +
                $"연결: {OVRInput.GetConnectedControllers()}\n" +
                $"활성: {OVRInput.GetActiveController()}\n" +
                $"Right 로컬 위치: {r:F3}\n" +
                $"Left  로컬 위치: {l:F3}\n" +
                $"Right 이동함: {rightEverMoved}\n" +
                $"Left  이동함: {leftEverMoved}";

            GUI.Box(new Rect(10, 10, 420, 180), info, style);
        }
    }
}
