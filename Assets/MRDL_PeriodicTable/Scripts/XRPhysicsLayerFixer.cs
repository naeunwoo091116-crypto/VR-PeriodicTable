// XR Interaction Toolkit Physics Layer Fixer
// ElementBox(8)와 Atoms(9) 레이어가 컨트롤러 레이캐스트 마스크에 포함되도록 보장합니다.
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// NearFarInteractor의 CurveInteractionCaster와 SphereInteractionCaster의
    /// physics layer mask에 ElementBox(8), Atoms(9) 레이어를 추가합니다.
    /// 이 스크립트가 없으면 원거리 레이캐스트가 Element 오브젝트에 닿지 않습니다.
    /// </summary>
    public class XRPhysicsLayerFixer : MonoBehaviour
    {
        void Awake()
        {
            // Layer 8 = ElementBox, Layer 9 = Atoms
            int addMask = (1 << 8) | (1 << 9);

            foreach (var caster in FindObjectsByType<CurveInteractionCaster>(FindObjectsSortMode.None))
            {
                int current = (int)caster.raycastMask;
                if ((current & addMask) != addMask)
                    caster.raycastMask = current | addMask;
            }

            foreach (var caster in FindObjectsByType<SphereInteractionCaster>(FindObjectsSortMode.None))
            {
                int current = (int)caster.physicsLayerMask;
                if ((current & addMask) != addMask)
                    caster.physicsLayerMask = current | addMask;
            }
        }
    }
}
