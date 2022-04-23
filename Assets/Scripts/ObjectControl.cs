using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles arbitrary object behaviour spawned by the object manager panel
/// </summary>
public class ObjectControl : MonoBehaviour
{
    public GameObject objectModel;
    private bool showObject = false;
    // Start is called before the first frame update
    void Start()
    {
        objectModel.SetActive(false);
    }

    /// <summary>
    /// Toggles the assigned object
    /// </summary>
    public void ToggleObject()
    {
        showObject = !showObject;
        objectModel.SetActive(showObject);
    }
}
