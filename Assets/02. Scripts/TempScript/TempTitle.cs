using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTitle : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Screen.fullScreen = false;
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
