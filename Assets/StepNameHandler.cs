using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tutorials
{
    public class StepNameHandler : MonoBehaviour
    {
        [SerializeField]
        private GameObject sceneNameEditor;
        [SerializeField]
        private Recorder recorder;
        [SerializeField]
        public TextMeshProUGUI stepNameInput;

        // Start is called before the first frame update
        void Start()
        {
            sceneNameEditor.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Edit the name of the scene and update the related UI components
        /// </summary>
        public void EditSceneName()
        {
            if (sceneNameEditor.activeSelf)
            {
                stepNameInput.text = stepNameInput.text.Trim();
                if (1 < stepNameInput.text.Length)
                {
                    Debug.Log($"Scene Name updated with {stepNameInput.text} and length {stepNameInput.text.Length}");
                    // Save the name to the animation file instance
                    recorder.NameCurrentAnimation(stepNameInput.text);
                }
                else
                {
                    Debug.Log("Scene Name not updated");
                }
                Debug.Log("Deactivate scene name editing");
                sceneNameEditor.SetActive(false);
            }
            else
            {
                Debug.Log("Activate scene name editing");
                sceneNameEditor.SetActive(true);
            }
        }
    }

}