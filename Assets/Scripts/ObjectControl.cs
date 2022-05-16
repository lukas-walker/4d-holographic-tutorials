using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles arbitrary object behaviour spawned by the object manager panel
/// </summary>
public class ObjectControl : MonoBehaviour
{
    public GameObject objectModel;
    // Start is called before the first frame update
    void Start()
    {
        objectModel.SetActive(false);
    }

    /// <summary>
    /// Set the position of the spawned object relative to the object manager panel
    /// </summary>
    public void SetManagerRelativePosition()
    {
        GameObject panel = GameObject.Find("SceneManagerPanel");
        if (panel == null) return;

        objectModel.transform.SetPositionAndRotation(panel.transform.position + new Vector3(.0f, .075f, .05f), objectModel.transform.rotation);
    }

    /// <summary>
    /// Toggles the assigned object
    /// </summary>
    public void ToggleObject()
    {
        SetManagerRelativePosition();
        objectModel.SetActive(!objectModel.activeSelf);
    }
}
