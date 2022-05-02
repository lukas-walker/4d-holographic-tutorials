// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// Class that initializes the appearance of the features panel according to the toggled states of the associated features
    /// </summary>
    internal class FeaturesPanelVisuals : MonoBehaviour
    {
        [SerializeField]
        private Interactable handMeshButton = null;
        [SerializeField]
        private Interactable handJointsButton = null;
        [SerializeField]
        private Interactable objectManagerPanel = null;

        private void Start()
        {
            MixedRealityHandTrackingProfile handProfile = null;
            if (CoreServices.InputSystem?.InputSystemProfile != null)
            {
                handProfile = CoreServices.InputSystem.InputSystemProfile.HandTrackingProfile;
            }
            handMeshButton.IsToggled = handProfile != null && handProfile.EnableHandMeshVisualization;
            handJointsButton.IsToggled = handProfile != null && handProfile.EnableHandJointVisualization;
            objectManagerPanel.IsToggled = false;
        }
    }
}
