using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class Mouseclick : MonoBehaviour, IPointerTriggerPressDownHandler {


  // Use this for initialization
  void Start () {
		
	}

  // Update is called once per frame
  void Update() {

    GameObject clickedGmObj = null;

    if (Input.GetMouseButtonDown(0)) {
      var hit = new RaycastHit();
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      //Debug.Log("111");
      if (Physics.Raycast(ray, out hit)) {

       }
      else {
        Debug.Log("didnt hit anything");
        //clickedGmObj.tag = "Not Selected";
      }
    }
  }

  public void OnPointerTriggerPressDown(XREventData eventData) {
    gameObject.tag = "Selected";
    Debug.Log("hit");
    Debug.Log(gameObject.tag);
  }
}
