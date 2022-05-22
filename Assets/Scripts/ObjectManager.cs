using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles arbitrary object behaviour spawned by the object manager panel
/// </summary>
public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private GameObject objectManagerPanel;

    [SerializeField]
    private GameObject objects;

    private Dictionary<string, GameObject> originalObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    private GameObject lastInteractedObject;

    /// <summary>
    /// Gives each instance a new ID
    /// </summary>
    private int n_clones;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in objects.transform)
        {
            child.gameObject.SetActive(false);
            originalObjects.Add(child.name, child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Spawns an instance of the assigned object
    /// </summary>
    public void SpawnObject(string type)
    {
        if(!originalObjects.TryGetValue(type, out GameObject objectModel))
        {
            Debug.Log($"Object type not available: {type}");
            return;
        }
        GameObject clone = Instantiate(
            objectModel,
            objectManagerPanel.transform.position + new Vector3(.0f, .1f, .0f),
            objectManagerPanel.transform.rotation * objectModel.transform.rotation,
            objectModel.transform.parent);
        clone.name = $"{objectModel.name}-{n_clones++}";
        clone.transform.localScale *= 2;
        spawnedObjects.Add(clone.name, clone);
        clone.SetActive(true);
    }

    /// <summary>
    /// Sets the tracked last interacted object to the last object hovered over (normally triggered in object manipulator)
    /// </summary>
    /// <param name="obj"></param>
    public void SetLastInteractedObject(GameObject obj)
    {
        lastInteractedObject = obj;
    }

    /// <summary>
    /// Removes the last interacted object from the scene
    /// </summary>
    public void RemoveLastInteractedObject()
    {
        spawnedObjects.Remove(lastInteractedObject.name);
        Destroy(lastInteractedObject);
        lastInteractedObject = null;
    }

    /// <summary>
    /// Gives the name of the last interacted object
    /// </summary>
    /// <returns></returns>
    public string GetLastInteractedObjectName()
    {
        if(lastInteractedObject == null)
        {
            return "None";
        }
        return lastInteractedObject.name;
    }

}
