using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tutorials
{
    public class StepNumber : MonoBehaviour
    {
        public LinkedListNode<AnimationWrapper> previousAnimation = null;
        public LinkedListNode<AnimationWrapper> currentAnimation = null;
        TextMeshPro textComponent = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            currentAnimation = FileHandler.AnimationListInstance.CurrentNode;
            if (previousAnimation == currentAnimation)
                return;
            previousAnimation = currentAnimation;
            textComponent = GetComponent<TextMeshPro>();
            if (textComponent == null)
                return;
            int index = currentAnimation == null ? -1 : FileHandler.AnimationListInstance.GetCurrentAnimationIndex();
            textComponent.text = $"{index + 1} / {FileHandler.AnimationListInstance.Count}";
        }
    }
}