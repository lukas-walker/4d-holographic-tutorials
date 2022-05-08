using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

        // Record Bar
        public PressableButtonHoloLens2 recordButton;
        public PressableButtonHoloLens2 addBoundingBoxButton;
        public GameObject boundingBox;

        // Playback Bar
        public PressableButtonHoloLens2 playButton;
        public TextMeshPro sceneNumberLabel;

        // Scene Name Bar
        public Button editNameButton;
        public GameObject sceneNameField;
        public InputField sceneNameField2;
        public TextMeshProUGUI sceneNameLabel;


        private bool isUpdatingName = false; // Specifies if scene name is currently being updated

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

            if (FileHandler.AnimationListInstance.CurrentNode == null)
            {
                // No current animation loaded/found
                sceneNumberLabel.text = "--/";
            }
            else
            {
                sceneNumberLabel.text = (FileHandler.AnimationListInstance.GetCurrentAnimationIndex() + 1).ToString() + "/";
            }

            sceneNumberLabel.text += FileHandler.AnimationListInstance.Count.ToString();
            sceneNumberLabel.text = "Step " + sceneNumberLabel.text;
            boundingBox.SetActive(false);
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
            Debug.Log("Record button pressed");
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
            // ColorBlock colors = recordButton.colors;
            // colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); // Light grey
            // recordButton.colors = colors;

            /*Component[] components = recordButton.GetComponentInChildren<PressableButtonHoloLens2>().GetComponents(typeof(Component));
            Debug.Log("GETTING COMPONENTS");
            foreach (Component component in components)
            {
                Debug.Log(component.ToString());
            }*/

            Debug.Log("Starting recording");
            //Debug.Log(recordButton.MovingButtonIconText);
            //recordButton.GetComponentInChildren<TextMeshPro>().text = "Stop Recording";
            //var colorTheme = this.GetComponent<Interactable>().ActiveThemes[0];
            //colorTheme.StateProperties[0].Values[0].Color = Color.green;
        }

        /// <summary>
        /// Called when recording is stopped. 
        /// </summary>
        private void OnStopRecording()
        {
            //ColorBlock colors = recordButton.colors;
            //colors.normalColor = new Color(0.9f, 0.0f, 0.0f, 1.0f); // Light grey
            //recordButton.colors = colors;

            //recordButton.GetComponentInChildren<TextMeshPro>().text = "Record";

            //playButton.GetComponentInChildren<Text>().text = "■";
            Debug.Log("Stopping recording");
            //recordButton.GetComponentInChildren<TextMeshPro>().text = "Record Step";
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
            Debug.Log("Next button pressed!");
            FileHandler.AnimationListInstance.Next();
        }

        /// <summary>
        /// Opens the previous animation entity in the editor. If there is no predecessor to the currently open animation entity, this method has no effect. 
        /// </summary>
        public void Previous()
        {
            Debug.Log("Previous button pressed!");
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
                playButton.GetComponentInChildren<TextMeshPro>().text = "▶";
            }
            else
            {
                player.Start();
                playButton.GetComponentInChildren<TextMeshPro>().text = "■";
            }


        }

        /// <summary>
        /// Edit the name of the scene and update the related UI components
        /// </summary>
        public void EditSceneName()
        {
            Debug.Log("Edit this scene name right now!");
            if (isUpdatingName)
            {
                isUpdatingName = false;

                // Save the name to the animation file instance 
                recorder.NameCurrentAnimation(sceneNameField2.text);

                // Update the scene name label
                sceneNameLabel.text = sceneNameField2.text;

                // Hide the input field
                sceneNameField.SetActive(false);

                // Change the edit button to say "✎"
                editNameButton.GetComponentInChildren<Text>().text = "Edit Name";
            }
            else
            {
                isUpdatingName = true;

                // Show the input field
                sceneNameField.SetActive(true);

                // Change edit button to say "Done"
                editNameButton.GetComponentInChildren<Text>().text = "Done";
            }
        }

        /// <summary>
        /// Add an object's bounding box to a scene that will function as a virtual representation of a real-world object
        /// </summary>
        public void AddBoundingBox()
        {
            Debug.Log("Adding a bounding box");
            if (boundingBox.activeSelf)
            {
                boundingBox.SetActive(false);
            }
            else
            {
                boundingBox.SetActive(true);
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