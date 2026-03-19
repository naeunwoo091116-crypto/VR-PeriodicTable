// XRInputModalityManager 타이밍 문제 수정 스크립트
// Meta XR Simulator에서 컨트롤러가 등록될 때 자동으로 감지하도록 보장합니다.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// XRInputModalityManager가 시작 시 컨트롤러를 감지하지 못하는 문제 수정.
    /// Start() 이후 짧은 대기 후 컨트롤러 연결 이벤트를 강제로 발생시킵니다.
    /// </summary>
    public class QuestXRModalityFixer : MonoBehaviour
    {
        [Tooltip("재시도 최대 횟수")]
        [SerializeField] private int maxRetries = 10;
        
        [Tooltip("재시도 간격 (초)")]
        [SerializeField] private float retryInterval = 0.3f;

        private System.Type modalityManagerType;
        private MonoBehaviour modalityManager;
        private System.Reflection.MethodInfo forceUpdateMethod;

        void Awake()
        {
            // XRInputModalityManager 타입 찾기
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                modalityManagerType = asm.GetType("UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager");
                if (modalityManagerType != null) break;
            }
        }

        IEnumerator Start()
        {
            if (modalityManagerType == null) yield break;
            
            // XRInputModalityManager 컴포넌트 찾기
            modalityManager = FindFirstObjectByType(modalityManagerType) as MonoBehaviour;
            
            if (modalityManager == null) yield break;
            
            // 활성화되어 있는지 확인, 아니면 활성화
            if (!modalityManager.enabled)
                modalityManager.enabled = true;

            // 컨트롤러 감지 재시도
            int retries = 0;
            while (retries < maxRetries)
            {
                yield return new WaitForSeconds(retryInterval);
                retries++;
                
                // 현재 모드 확인
                var leftModeField = modalityManagerType.GetField("m_LeftInputMode", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var rightModeField = modalityManagerType.GetField("m_RightInputMode",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                var leftMode = leftModeField?.GetValue(modalityManager);
                var rightMode = rightModeField?.GetValue(modalityManager);
                
                bool leftNone = leftMode?.ToString() == "None";
                bool rightNone = rightMode?.ToString() == "None";
                
                if (!leftNone && !rightNone)
                {
                    Debug.Log($"[QuestXRModalityFixer] 양쪽 컨트롤러 감지 완료: Left={leftMode}, Right={rightMode}");
                    yield break;
                }
                
                // 장치 목록 확인 및 강제 이벤트 발생
                var devices = new List<InputDevice>();
                InputDevices.GetDevicesWithRole(InputDeviceRole.LeftHanded, devices);
                InputDevices.GetDevicesWithRole(InputDeviceRole.RightHanded, devices);
                
                if (devices.Count > 0)
                {
                    Debug.Log($"[QuestXRModalityFixer] {devices.Count}개 장치 발견. 모달리티 매니저 재초기화 시도...");
                    ForceModalityRescan();
                }
                else if (leftNone || rightNone)
                {
                    // 장치가 없으면 컨트롤러 오브젝트를 직접 활성화
                    ForceControllersActive();
                }
            }
            
            Debug.Log($"[QuestXRModalityFixer] 최대 재시도 완료. 컨트롤러 강제 활성화 중...");
            ForceControllersActive();
        }
        
        private void ForceModalityRescan()
        {
            if (modalityManager == null || modalityManagerType == null) return;
            
            // XRInputModalityManager를 재활성화해서 초기화 다시 실행
            modalityManager.enabled = false;
            modalityManager.enabled = true;
        }
        
        private void ForceControllersActive()
        {
            if (modalityManager == null || modalityManagerType == null) return;
            
            // m_LeftController와 m_RightController를 직접 활성화
            var leftCtrlField = modalityManagerType.GetField("m_LeftController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rightCtrlField = modalityManagerType.GetField("m_RightController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var leftCtrl = leftCtrlField?.GetValue(modalityManager) as GameObject;
            var rightCtrl = rightCtrlField?.GetValue(modalityManager) as GameObject;
            
            if (leftCtrl != null && !leftCtrl.activeSelf)
            {
                leftCtrl.SetActive(true);
                Debug.Log("[QuestXRModalityFixer] Left Controller 강제 활성화 ✓");
            }
            if (rightCtrl != null && !rightCtrl.activeSelf)
            {
                rightCtrl.SetActive(true);
                Debug.Log("[QuestXRModalityFixer] Right Controller 강제 활성화 ✓");
            }
        }
    }
}
