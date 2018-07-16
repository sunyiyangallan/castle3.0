using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class MoveCamera : MonoBehaviour {

  public XRControllerModule module; // pass in the left hand, so only the thumbthick on the left hand will move the camera
  public float speed;

	// Update is called once per frame
	void Update () {
    var touchpadAxis = module.GetTouchpadAxis();
    var thumbstickAxis = module.GetThumbstickAxis();
    if (Mathf.Abs(thumbstickAxis.y) > 0.2) {
      transform.Translate(speed * thumbstickAxis.y * module.transform.forward * Time.deltaTime);
    }
    if (Mathf.Abs(thumbstickAxis.x) > 0.2) {
      Vector3 A = Quaternion.AngleAxis(90, Vector3.up) * module.transform.forward;
      transform.Translate(speed * thumbstickAxis.x * A * Time.deltaTime);
      
    }
  }
}
