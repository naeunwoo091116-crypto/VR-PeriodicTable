// Meta Quest 카메라 리그 설정 스크립트
// OVRCameraRig 또는 XR Origin을 기반으로 씬 카메라를 초기화합니다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// Meta Quest에서 씬의 카메라 위치를 초기화하고,
    /// HoloLens용이었던 OptimizeSceneforDeviceType 역할을 대신합니다.
    /// </summary>
    public class QuestCameraRig : MonoBehaviour
    {
        [Tooltip("주기율표 컨테이너 오브젝트")]
        public GameObject containerObject;

        [Header("Quest 기본 위치 (불투명 디스플레이 = 일반 VR)")]
        public Vector3 opaqueDisplayPosition = new Vector3(0.05f, 1.2f, 0.50f);

        [Header("투명 디스플레이 위치 (패스스루 MR 모드)")]
        public Vector3 transparentDisplayPosition = new Vector3(0.05f, -0.65f, 2.00f);

        void Start()
        {
            if (containerObject == null) return;

            bool isOpaque = IsDisplayOpaque();

            if (isOpaque)
            {
                // Quest VR 모드 - 불투명 디스플레이
                containerObject.transform.position = opaqueDisplayPosition;
            }
            else
            {
                // Quest MR 패스스루 모드 - 투명 디스플레이
                containerObject.transform.position = transparentDisplayPosition;
                RenderSettings.skybox = null;
            }
        }

        private bool IsDisplayOpaque()
        {
            var displaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displaySubsystems);
            if (displaySubsystems.Count > 0)
                return displaySubsystems[0].displayOpaque;
            return true; // XR 미연결 시 기본값: 불투명(VR)
        }
    }
}
