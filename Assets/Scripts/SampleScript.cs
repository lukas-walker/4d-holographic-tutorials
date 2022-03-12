using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The object this script targets.")]
    private GameObject someGameObject;

    private bool red = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick() {
        Debug.Log("Click received.");

        Renderer renderer = gameObject.GetComponent<Renderer>();
        // could also use this.gameObject instead, since this script is attached to the cube gameObject anyway.
        

        if (!red) renderer.material.SetColor("_Color", Color.red);
        else renderer.material.SetColor("_Color", Color.green);

        red = !red;
    }


}
