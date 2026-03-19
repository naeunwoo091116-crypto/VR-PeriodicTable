using UnityEngine;
using UnityEngine.InputSystem;

namespace HoloToolkit.MRDL.PeriodicTable
{
    /// <summary>
    /// Enables XRI Default Input Actions at startup so that
    /// head tracking and controllers work with Meta XR Simulator.
    /// </summary>
    public class XRInputActionsEnabler : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actionAsset;

        void Awake()
        {
            if (actionAsset != null)
            {
                actionAsset.Enable();
                Debug.Log($"[XRInputActionsEnabler] Enabled: {actionAsset.name}");
            }
            else
            {
                Debug.LogWarning("[XRInputActionsEnabler] No action asset assigned!");
            }
        }
    }
}
