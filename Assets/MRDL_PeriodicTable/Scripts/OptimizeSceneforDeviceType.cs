//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace HoloToolkit.MRDL.PeriodicTable
{
    public class OptimizeSceneforDeviceType : MonoBehaviour
    {
        public GameObject containerObject;

        void Start()
        {
            // Check if the display is opaque (Immersive HMD) or transparent (HoloLens)
            bool isDisplayOpaque = true;
            var displaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displaySubsystems);
            if (displaySubsystems.Count > 0)
            {
                isDisplayOpaque = displaySubsystems[0].displayOpaque;
            }

            if (isDisplayOpaque)
            {
                // Optimize the default postion of the objects for Immersive HMD (Meta Quest)
                // FloorLevel tracking: camera (eye) is at ~Y=1.70m above floor
                // Table container Y=1.60 → element grid centered at ~Y=1.50 (slightly below eye)
                // Z=-1.50 → element grid at world Z≈0.30 (1.8m in front of camera)
                containerObject.transform.position = new Vector3(0.05f, 1.60f, -1.50f);
            }
            else
            {
                // Optimize the default postion of the objects for HoloLens
                containerObject.transform.position = new Vector3(0.05f, -0.65f, 0.50f);

                // Remove skybox for HoloLens
                RenderSettings.skybox = null;
            }
        }
    }
}
