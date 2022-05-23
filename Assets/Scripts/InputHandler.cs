using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using TMPro;

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

        void OnDestroy()
        {
            FileHandler.SaveAnimationList();
        }

        public TextMeshPro recordingCountdownText;

        private string countdownText = "";
        private string shouldStartRecord = "no"; // Using strings because locks don't work on bools
        private CancellationTokenSource cancelRecordingCountdownToken;

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

            if (recordingCountdownText.text != countdownText)
            {
                // Checking value first because it might be more efficient than setting label text on every update
                recordingCountdownText.text = countdownText;
            }

            if (shouldStartRecord == "yes")
            {
                // Start the recording from the main thread
                shouldStartRecord = "no";
                RecordAnimation();
            }


        }

        private void Start()
        {
            // adding all the listeners to UnityEvents, C# Events and Actions
            recorder.OnRecordingStarted.AddListener(OnStartRecording);
            recorder.OnRecordingStopped.AddListener(OnStopRecording);
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
                if (countdownText != "")
                {
                    // Countdown is currently running and user wants to cancel
                    cancelRecordingCountdownToken.Cancel();
                    countdownText = "";
                }
                else
                {
                    // Countdown has not started so user is wanting to start recording
                    cancelRecordingCountdownToken = new CancellationTokenSource();
                    Thread t = new Thread(() => StartCountdownThenRecord(cancelRecordingCountdownToken));
                    t.Start();
                }
                
            }
        }

        public void StartCountdownThenRecord(CancellationTokenSource ct)
        {
            for (int i = 3; i > 0; i--)
            {
                if (ct.IsCancellationRequested)
                {
                    // User has requested to not record
                    return;
                }

                lock (countdownText)
                {
                    countdownText = i.ToString();
                }
                var canceled = ct.Token.WaitHandle.WaitOne(1000);
                if (canceled)
                {
                    return;
                }
            }

            lock (countdownText)
            {
                countdownText = "";
            }

            lock (shouldStartRecord)
            {
                // Tell the main thread to start recording
                shouldStartRecord = "yes";
            }

            cancelRecordingCountdownToken.Dispose();
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
        /// Creates a new animation appended to the animation list
        /// </summary>
        public void CreateNewAnimationWrapper()
        {
            recorder.CreateNewAnimationWrapper();
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
        }

        /// <summary>
        /// Called when recording is stopped. 
        /// </summary>
        private void OnStopRecording()
        {
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
            }
            else
            {
                player.PlayCurrent();
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