using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles arbitrary object behaviour spawned by the object manager panel
/// </summary>
public class ObjectControl : MonoBehaviour
{
    [SerializeField]
    private GameObject objectModel;
    [SerializeField]
    private GameObject objectManagerPanel;

    /// <summary>
    /// Gives each instance a new ID
    /// </summary>
    private int n_clones;
    
    // Start is called before the first frame update
    void Start()
    {
        objectModel.SetActive(false);
    }

    /// <summary>
    /// Spawns an instance of the assigned object
    /// </summary>
    public void SpawnObject()
    {
        GameObject clone = Instantiate(
            objectModel,
            objectManagerPanel.transform.position + new Vector3(.0f, .1f, .0f),
            objectManagerPanel.transform.rotation * objectModel.transform.rotation,
            objectModel.transform.parent);
        Destroy(clone.GetComponent<ObjectControl>());
        clone.name = $"{objectModel.name}-{n_clones++}";
        clone.transform.localScale *= 3;
        clone.SetActive(true);
    }
}
