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
        public TMP_InputField stepNameInputField;

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
                stepNameInputField.text = stepNameInputField.text.Trim();
                if (stepNameInputField.text != string.Empty)
                {
                    // Save the name to the animation file instance
                    recorder.NameCurrentAnimation(stepNameInputField.text);
                    stepNameInputField.text = "";
                }
                sceneNameEditor.SetActive(false);
            }
            else
            {
                sceneNameEditor.SetActive(true);
            }
        }
    }

}