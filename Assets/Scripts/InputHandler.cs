﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace Tutorials
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField]
        private Recorder recorder;

        [SerializeField]
        private Player player;

        [SerializeField]
        private Transform animationSpecificPointOfReference;


        public Button recordButton;
        public Button playButton;

        void OnDestroy()
        {
            FileHandler.SaveAnimationList();
        }
        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.I))
            {
                RecordAnimation();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                SaveAnimation();
            }
            else if (Input.GetKeyDown(KeyCode.P)) {
                CreateNewAnimationWrapper();
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                CloseAnimation();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("Pressed K for Previous");
                Previous();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("Pressed L for Next");
                Next();
            }
#endif
        }

        private void Start()
        {
            // adding all the listeners to UnityEvents, C# Events and Actions
            recorder.OnRecordingStarted.AddListener(OnStartRecording);
            recorder.OnRecordingStopped.AddListener(OnStopRecording);
        }

        /// <summary>
        /// Creates a new animation wrapper that will be opened in the Editor. 
        /// The animation specific point of reference will be reset to the global origin that is either at the MixedReality
        /// space's origin or the origin that has been set through QR Code calibration. 
        /// </summary>
        public void CreateNewAnimationWrapper()
        {
            animationSpecificPointOfReference.localPosition = Vector3.zero;
            animationSpecificPointOfReference.localRotation = Quaternion.identity;
            FileHandler.AnimationListInstance.CreateNewAnimationEntity(null, animationSpecificPointOfReference);
        }

        /// <summary>
        /// Record UI button pressed; take correct action to start/stop
        /// </summary>
        public void RecordButtonAction()
        {
            // TODO: Grey-out/disable non-recording buttons while recording
            if (recorder.IsRecording)
            {
                 SaveAnimation();
            }
            else
            {
                RecordAnimation();
            }
        }

        /// <summary>
        /// Starts the recording if there isn't already recorded content in the currently open animation entity. 
        /// </summary>
        public void RecordAnimation()
        {

            if (FileHandler.AnimationListInstance.CurrentNode == null)
            {
                CreateNewAnimationWrapper();
            }

            recorder.StartRecording();
            
        }

        /// <summary>
        /// Called when the user makes a choice in the overwrite dialog. If the user chooses yes (i.e. the recording should start, overwriting the current content), the recording service is activated. 
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnOverwriteDialogClosed(DialogResult obj)
        {
            if (obj.Result == DialogButtonType.Yes)
            {
                recorder.StartRecording();

            }
        }

        /// <summary>
        /// Called when recording starts.
        /// </summary>
        private void OnStartRecording()
        {
            ColorBlock colors = recordButton.colors;
            colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); // Light grey
            recordButton.colors = colors;

            recordButton.GetComponentInChildren<Text>().text = "Stop Recording";

            Debug.Log("Starting recording");
        }

        /// <summary>
        /// Called when recording is stopped. 
        /// </summary>
        private void OnStopRecording()
        {
            ColorBlock colors = recordButton.colors;
            colors.normalColor = new Color(0.9f, 0.0f, 0.0f, 1.0f); // Light grey
            recordButton.colors = colors;

            recordButton.GetComponentInChildren<Text>().text = "Record";

            playButton.GetComponentInChildren<Text>().text = "■";
            Debug.Log("Stopping recording");
            SaveAnimation();
        }

        /// <summary>
        /// Saves the animation that has just been recorded. This activates the loading rotating orbs on the recording button to inform the user that the recording is currently being saved. 
        /// </summary>
        public void SaveAnimation()
        {
            recorder.SaveRecordedInput();
        }

        /// <summary>
        /// Closes the animation that is currently open in the editor. This animation will be removed from the animations list. 
        /// </summary>
        public void CloseAnimation()
        {
            recorder.CloseAnimation();
        }

        /// <summary>
        /// Cancels the recording and discards the recorded content. Note that this method is only available to the user through voice command. 
        /// </summary>
        public void Cancel()
        {
            recorder.Cancel();
        }

        /// <summary>
        /// Opens the next animation entity in the editor. If there is no successor to the currently open animation entity, this method has no effect. 
        /// </summary>
        public void Next()
        {
            FileHandler.AnimationListInstance.Next();
        }

        /// <summary>
        /// Opens the previous animation entity in the editor. If there is no predecessor to the currently open animation entity, this method has no effect. 
        /// </summary>
        public void Previous()
        {
            FileHandler.AnimationListInstance.Previous();
        }

        /// <summary>
        /// Resets the current animation to the start frame (100% in the loading bar).
        /// </summary>
        public void StartAgain()
        {
            playButton.GetComponentInChildren<Text>().text = "■";
            player.StartAgain();
        }

        /// <summary>
        /// Play/Stop the current animation when the play button is pressed
        /// </summary>
        public void PlayButtonAction()
        {
            if (player.IsPlaying())
            {
                player.Stop();
                playButton.GetComponentInChildren<Text>().text = "▶";
            }
            else
            {
                player.Start();
                playButton.GetComponentInChildren<Text>().text = "■";
            }


        }

        /// <summary>
        /// Should be called when the user changes the position or rotation of the animation specific point of reference changed.
        /// </summary>
        /// <param name="animationSpecificPointOfReference">The animation specific point of reference.</param>
        public void OnAnimationSpecificPointOfReferenceChanged()
        {
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().position_x = animationSpecificPointOfReference.transform.localPosition.x;
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().position_y = animationSpecificPointOfReference.transform.localPosition.y;
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().position_z = animationSpecificPointOfReference.transform.localPosition.z;

            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().rotation_x = animationSpecificPointOfReference.transform.localRotation.x;
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().rotation_y = animationSpecificPointOfReference.transform.localRotation.y;
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().rotation_z = animationSpecificPointOfReference.transform.localRotation.z;
            FileHandler.AnimationListInstance.GetCurrentAnimationWrapper().rotation_w = animationSpecificPointOfReference.transform.localRotation.w;
        }
    }
}