using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorials.ResearchMode
{

    public class KeyboardCommand : MonoBehaviour
    {
        ResearchModeVideoStream rm;

        void Start()
        {
            rm = GetComponent<ResearchModeVideoStream>();
        }

        // Update is called once per frame
        void Update()
        {
#if ENABLE_WINMD_SUPPORT
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rm.SaveAHATSensorDataEvent();
        }
#endif
        }
    }
}