// 레이아웃 버튼들에 XRI(XR Interaction Toolkit) 인터랙션을 런타임에 연결하는 스크립트.
// LayoutStyleButtons 또는 LayoutStyleButtonsHandMenu GameObject에 부착하여 사용합니다.
// 자식 오브젝트 중 ButtonPlane, ButtonCylinder, ButtonSphere, ButtonRadial을 탐색하고
// XRSimpleInteractable + BoxCollider를 자동으로 추가하여 VR 레이로 클릭 가능하게 합니다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class LayoutButtonsXRLinker : MonoBehaviour
    {
        [Tooltip("LayoutStyleChanger 컴포넌트 참조. 비워두면 씬에서 자동 탐색합니다.")]
        public LayoutStyleChanger layoutStyleChanger;

        // 버튼 이름 → LayoutStyleChanger 메서드 매핑
        private Dictionary<string, System.Action> buttonActions;

        void Awake()
        {
            if (layoutStyleChanger == null)
                layoutStyleChanger = FindFirstObjectByType<LayoutStyleChanger>();

            if (layoutStyleChanger == null)
            {
                Debug.LogError("[LayoutButtonsXRLinker] LayoutStyleChanger를 찾을 수 없습니다. 씬에 LayoutStyleChanger가 있는지 확인하세요.");
                return;
            }

            buttonActions = new Dictionary<string, System.Action>
            {
                { "ButtonPlane",    layoutStyleChanger.ChangeLayoutStylePlane },
                { "ButtonCylinder", layoutStyleChanger.ChangeLayoutStyleCylinder },
                { "ButtonSphere",   layoutStyleChanger.ChangeLayoutStyleSphere },
                { "ButtonRadial",   layoutStyleChanger.ChangeLayoutStyleRadial },
            };
        }

        void Start()
        {
            if (buttonActions == null) return;
            SetupButtons(transform);
        }

        private void SetupButtons(Transform root)
        {
            foreach (var kvp in buttonActions)
            {
                Transform btn = FindDeep(root, kvp.Key);
                if (btn == null)
                {
                    Debug.LogWarning($"[LayoutButtonsXRLinker] '{kvp.Key}' 버튼 오브젝트를 찾지 못했습니다.");
                    continue;
                }

                // XRSimpleInteractable 추가 (없을 경우)
                var interactable = btn.GetComponent<XRSimpleInteractable>();
                if (interactable == null)
                    interactable = btn.gameObject.AddComponent<XRSimpleInteractable>();

                // Collider 추가 (없을 경우) — 버튼 크기에 맞는 BoxCollider
                if (btn.GetComponent<Collider>() == null)
                {
                    var col = btn.gameObject.AddComponent<BoxCollider>();
                    col.size = new Vector3(0.032f, 0.032f, 0.01f);
                    col.center = Vector3.zero;
                }

                // selectEntered 이벤트 → LayoutStyleChanger 메서드 연결
                System.Action action = kvp.Value;
                interactable.selectEntered.AddListener(_ => action.Invoke());

                Debug.Log($"[LayoutButtonsXRLinker] '{btn.name}' (경로: {GetPath(btn)}) → {action.Method.Name} 연결 완료");
            }
        }

        /// <summary>계층 구조를 재귀적으로 탐색하여 이름이 일치하는 Transform을 반환합니다.</summary>
        private Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null && t.parent != transform)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
