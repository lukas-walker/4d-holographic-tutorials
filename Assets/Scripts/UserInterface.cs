using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//namespace Tutorials
//{
    public class UserInterface : MonoBehaviour
    {

        /*public Button button;

        // Start is called before the first frame update
        void Start()
        {
            Button btn = button.GetComponent<Button>();
            btn.onClick.AddListener(NextStep);
        }

        // Update is called once per frame
        void Update()
        {

        }*/

        public void NextStep()
        {
            Debug.Log("Traveling to the next scene!");

        }

        public void PreviousStep()
        {
            Debug.Log("Traveling to the previous scene!");

        }

        public void PlayStep()
        {

        }

        public void RenameStep()
        {

        }
    }
//}

