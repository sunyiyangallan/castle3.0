using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class PointingLine : MonoBehaviour {
    public XRControllerModule module;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (module.xrEventData.currentRaycast != null) {
            GameObject myLine = new GameObject();
            myLine.transform.position = module.transform.position;
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            // lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.SetColors(Color.red, Color.white);
            lr.SetWidth(0.01f, 0.01f);
            lr.SetPosition(0, module.transform.position);
            lr.SetPosition(1, module.xrEventData.worldPosition);
            GameObject.Destroy(myLine, 0.03f);
        }
    }
}
