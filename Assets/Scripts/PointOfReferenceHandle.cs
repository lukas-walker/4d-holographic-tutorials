using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOfReferenceHandle : MonoBehaviour
{
    [SerializeField]
    private GameObject handle;

    private bool handleIsActive = false;

    // Start is called before the first frame update
    void Start()
    {
        handle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void toggleHandle()
    {
        if (!handleIsActive)
        {
            handle.SetActive(true);
            handleIsActive = true;
        }
        else { 
            handle.SetActive(false);
            handleIsActive = false;
        }
    }
}
