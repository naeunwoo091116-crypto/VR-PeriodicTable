// 컨트롤러 Visual이 숨겨지지 않도록 강제 유지하는 스크립트
using UnityEngine;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// XRInputModalityManager가 컨트롤러 Visual을 숨기는 것을 방지합니다.
    /// </summary>
    public class ForceControllerVisible : MonoBehaviour
    {
        [SerializeField] private GameObject controllerVisual;

        void Start()
        {
            if (controllerVisual == null)
                controllerVisual = transform.Find("Right Controller Visual")?.gameObject
                               ?? transform.Find("Left Controller Visual")?.gameObject;
        }

        void LateUpdate()
        {
            if (controllerVisual != null && !controllerVisual.activeSelf)
            {
                controllerVisual.SetActive(true);
            }
        }
    }
}
