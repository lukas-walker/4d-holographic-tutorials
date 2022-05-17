using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Serialization;
using UnityEngine.Events;

namespace Tutorials
{



    /// <summary>
    /// Represents the state of the animations that are currently open in the editor (i.e. visible to the user). The instance of AnimationList is handled by the FileHandler class through a static reference and appears only once in the context of the active runtime environment. 
    /// Whenever the state changes (i.e. animations are added, removed, etc.), this instance invokes the event CurrentAnimationChanged to let listeners (like the PlaybackService) know that the current animation has changed.
    /// This class can be serialized and will be saved to the persistend data path in the datafile.xml. 
    /// </summary>
    [XmlRootAttribute("AnimationList", Namespace = "http://rimon-ar.ch/", IsNullable = false)]
    public class AnimationList
    {
        /// <summary>
        /// Contains a list of all animations that are currently loaded in the editor.
        /// In the serialization process this field will be ignored. Instead, all data will be copied to AnimationList.animationsArray first, which will then be serialized.
        /// </summary>
        /// <seealso cref="AnimationList.animationsArray" />
        [XmlIgnore]
        private LinkedList<AnimationWrapper> animations;


        /// <summary>
        /// Only used to serialize the field AnimationList.animations while storing and loading ManualData.
        /// Note: this field should never be referenced as it will only carry meaning while storing and loading data. 
        /// </summary>
        [XmlArrayAttribute("animations")]
        public AnimationWrapper[] animationsArray;

        /// <summary>
        /// Helper method to copy data from animations to animationsArray
        /// </summary>
        public void CopyLinkedListToArray()
        {
            animationsArray = new AnimationWrapper[animations.Count];

            animations.CopyTo(animationsArray, 0);
        }

        /// <summary>
        /// Helper method to copy data from animationsArray to animations 
        /// </summary>
        public void CopyArrayToLinkedList()
        {
            animations = new LinkedList<AnimationWrapper>();


            if (animationsArray != null)
            {
                foreach (AnimationWrapper animationEntity in animationsArray)
                {
                    LinkedListNode<AnimationWrapper> newNode = new LinkedListNode<AnimationWrapper>(animationEntity);
                    animations.AddLast(newNode);
                }
            }

            animationsArray = null;

            CurrentAnimationChanged.Invoke();
        }

        [XmlIgnore]
        private LinkedListNode<AnimationWrapper> currentNode;

        /// <summary>
        /// Contains the animation in AnimationList.animations that is currently active and open in the editor. Changes to the loaded animation in the editor will be executed on the animation in this node. 
        /// </summary>
        [XmlIgnore]
        public LinkedListNode<AnimationWrapper> CurrentNode
        {
            get
            {
                if (currentNode == null)
                {
                    if (animations.Count > 0)
                    {
                        currentNode = animations.First;
                    }
                }
                return currentNode;
            }
            set => currentNode = value;
        }


        public int Count
        {
            get { return animations.Count; }
        }

        public int GetCurrentAnimationIndex()
        {
            int i = 0;
            for (LinkedListNode<AnimationWrapper> node = animations.First; node != null; node = node.Next)
            {
                if (node == currentNode)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Access method for the animation entity that is currently active and open in the editor
        /// </summary>
        /// <returns>Returns the animation entity that is currently active and open in the editor</returns>
        public AnimationWrapper GetCurrentAnimationWrapper()
        {
            if (CurrentNode == null) return null;
            return CurrentNode.Value;
        }


        /// <summary>
        /// Will be invoked whenever the state of the AnimationList instance changes, i.e. if animations are added or removed or if CurrentNode changes its value. 
        /// </summary>
        [XmlIgnore]
        public UnityEvent CurrentAnimationChanged = new UnityEvent();

        /// <summary>
        /// Will be invoke when an the file handler completes storing an animation to the local persistant datapath. 
        /// </summary>
        [XmlIgnore]
        public UnityEvent AnimationStoredLocally = new UnityEvent();

        /// <summary>
        /// This class' only constructor: Initializes a new instance of the <see cref="AnimationList"/> class.
        /// </summary>
        public AnimationList()
        {
            animations = new LinkedList<AnimationWrapper>();
        }


        /// <summary>
        /// Adds an empty animation entity to AnimationList.animations and updates CurrentNode to reference the newly added animations.
        /// CurrentAnimationChanged will be invoked. 
        /// Note: The new animation will always be added right after the one that is currently active and open in the editor. 
        /// Note: This method creates a new animation entity and then calls AddAnimationEntity with that new entity as a parameter. Hence CurrentAnimationChanged will be invoked. 
        /// </summary>
        /// <param name="inputAnimation">The input animation that the new animation entity should reference.</param>
        /// <param name="animationSpecificPointOfReference">The animation specific point of reference. (Will be a part of the animation entities position and rotation definition, s.t. the position and rotation relative to the origin can be updated later. Default value: (0,0,0),(0,0,0,1)</param>
        /// <returns>The newly added animation entity.</returns>
        public AnimationWrapper CreateNewAnimationEntity(InputAnimation inputAnimation, Transform animationSpecificPointOfReference)
        {
            AnimationWrapper animationWrapper = new AnimationWrapper();

            animationWrapper.Animation = inputAnimation;

            // if there is already content available, fill the new entity with it (else leave it blank, s.t. it can be filled by recording a new animation)
            if (inputAnimation != null)
            {
                string blobFileName = FileHandler.GetBlobFileName();

                try
                {
                    FileHandler.StoreBlobFileLocally(inputAnimation, FileHandler.GetFilePath(blobFileName));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return null;
                }

                animationWrapper.Name = blobFileName;
                animationWrapper.Description = "Unnamed";
                animationWrapper.position_x = animationSpecificPointOfReference.localPosition.x;
                animationWrapper.position_y = animationSpecificPointOfReference.localPosition.y;
                animationWrapper.position_z = animationSpecificPointOfReference.localPosition.z;

                animationWrapper.rotation_x = animationSpecificPointOfReference.localRotation.x;
                animationWrapper.rotation_y = animationSpecificPointOfReference.localRotation.y;
                animationWrapper.rotation_z = animationSpecificPointOfReference.localRotation.z;
                animationWrapper.rotation_w = animationSpecificPointOfReference.localRotation.w;
            }

            AddAnimationEntity(animationWrapper);

            return animationWrapper;
        }


        /// <summary>
        /// Overwrites the blob file reference in the currently loaded animation entity. 
        /// CurrentAnimationChanged will be invoked
        /// Note: blob files cannot be changed, but they can be replaced by changing the reference (animationWrapper.BlobFileName) to a different blob file. 
        /// </summary>
        /// <param name="inputAnimation">The input animation that should replace the current content of the loaded animation entity.</param>
        /// <param name="animationSpecificPointOfReference">The animation specific point of reference.(Will be a part of the animation entities position and rotation definition, s.t. the position and rotation relative to the origin can be updated later. Default value: (0,0,0),(0,0,0,1)</param></param>
        public void OverwriteAnimationData(InputAnimation inputAnimation, Transform animationSpecificPointOfReference)
        {
            if (inputAnimation != null && CurrentNode.Value != null)
            {
                string blobFileName = FileHandler.GetBlobFileName();

                CurrentNode.Value.Animation = inputAnimation;

                try
                {
                    FileHandler.StoreBlobFileLocally(inputAnimation, FileHandler.GetFilePath(blobFileName));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return;
                }

                CurrentNode.Value.Name = blobFileName;
                CurrentNode.Value.position_x = animationSpecificPointOfReference.localPosition.x;
                CurrentNode.Value.position_y = animationSpecificPointOfReference.localPosition.y;
                CurrentNode.Value.position_z = animationSpecificPointOfReference.localPosition.z;

                CurrentNode.Value.rotation_x = animationSpecificPointOfReference.localRotation.x;
                CurrentNode.Value.rotation_y = animationSpecificPointOfReference.localRotation.y;
                CurrentNode.Value.rotation_z = animationSpecificPointOfReference.localRotation.z;
                CurrentNode.Value.rotation_w = animationSpecificPointOfReference.localRotation.w;
            }

            CurrentAnimationChanged.Invoke();

            FileHandler.SaveAnimationList();
        }

        /// <summary>
        /// Adds an animation wrapper that already exists, i.e. that was loaded from local memory or from cloud storage. 
        /// CurrentAnimationChanged will be invoked
        /// </summary>
        /// <param name="AnimationWrapper">The wrapper that is already available.</param>
        public void AddAnimationEntity(AnimationWrapper animationWrapper)
        {
            LinkedListNode<AnimationWrapper> newNode = new LinkedListNode<AnimationWrapper>(animationWrapper);

            if (animations.Count == 0)
            {
                animations.AddLast(newNode);
            }
            else
            {
                animations.AddAfter(CurrentNode, newNode);
            }

            CurrentNode = newNode;

            CurrentAnimationChanged.Invoke();

            FileHandler.SaveAnimationList();
        }

        /// <summary>
        /// Removes a the CurrentNode from AnimationList.animations and loads the previous node. If there is no previous node, the next node will be loaded. If there is no other node, CurrentNode will be set to null. 
        /// CurrentAnimationChanged will be invoked
        /// </summary>
        public void RemoveCurrentAnimation()
        {
            if (CurrentNode != null)
            {
                LinkedListNode<AnimationWrapper> tempNode = CurrentNode;

                if (CurrentNode.Previous != null) CurrentNode = CurrentNode.Previous;
                else if (CurrentNode.Next != null) CurrentNode = CurrentNode.Next;
                else CurrentNode = null;

                animations.Remove(tempNode);

                FileHandler.SaveAnimationList();

                CurrentAnimationChanged.Invoke();
            }
        }


        /// <summary>
        /// Jumps one node forward in the animations list. The CurrentNode will go on to reference its successor. 
        /// CurrentAnimationChanged will be invoked. 
        /// Note: If there is no next node (i.e. there is no animation in the animations list or the current loaded animation is last in the list), this method has no effect. 
        /// </summary>
        /// <returns>Returns the animation entity that is currently active and open in the editor after the node has been changed to next.</returns>       
        public AnimationWrapper Next()
        {
            if (CurrentNode.Next != null)
            {
                CurrentNode = CurrentNode.Next;
                CurrentAnimationChanged.Invoke();
            }

            return CurrentNode?.Value;

        }

        /// <summary>
        /// Jumps one node backward in the animations list. The CurrentNode will go on to reference its predecessor. 
        /// CurrentAnimationChanged will be invoked. 
        /// Note: If there is no previous node (i.e. there is no animation in the animations list or the current loaded animation is first in the list), this method has no effect. 
        /// </summary>
        /// <returns>Returns the animation entity that is currently active and open in the editor after the node has been changed to previous.</returns>
        public AnimationWrapper Previous()
        {
            if (CurrentNode.Previous != null)
            {
                CurrentNode = CurrentNode.Previous;
                CurrentAnimationChanged.Invoke();
            }

            return CurrentNode?.Value;
        }

        /// <summary>
        /// Converts to string in a rudimentary way. The currently active animation entity that is loaded in the editor will be preceded with ">>>"
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance in human readable form. 
        /// </returns>
        public override string ToString()
        {
            string list = "Open Animations: \n";

            foreach (AnimationWrapper wrapper in animations)
            {
                if (wrapper.Equals(currentNode.Value)) list += ">>>" + wrapper.Name + "\n";
                else list += wrapper.Name + "\n";
            }

            return list;
        }

        /// <summary>
        /// Returns the number of the currently active animation entity in the animation list. 
        /// This value can be used easily to determine if the correct animation in a sequence is loaded. 
        /// </summary>
        /// <returns>Returns the number of the currently active animation entity in the animation list<returns>
        public int CurrentIndex()
        {
            if (currentNode == null) return 0;

            else
            {
                var count = 1;
                for (var node = animations.First; node != null; node = node.Next, count++)
                {
                    if (currentNode.Value.Equals(node.Value))
                        return count;
                }
            }
            return 0;
        }
    }
}