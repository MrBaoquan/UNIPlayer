using System.Collections;
using System.Collections.Generic;
using IOToolkit;
using UnityEngine;
public class Test_IOToolkit : MonoBehaviour {
    // Start is called before the first frame update
    void Start () {
        IODeviceController.Load ();
    }

    // Update is called once per frame
    void Update () {
        IODeviceController.Update ();

        if (IODeviceController.GetIODevice ("Standard").GetKeyDown (IOKeyCode.S)) {
            Debug.Log ("S");
        }
    }

    void OnDestroy () {
        IODeviceController.UnLoad ();
    }
}