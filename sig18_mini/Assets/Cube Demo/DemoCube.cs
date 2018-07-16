using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class DemoCube : MonoBehaviour, IPointerTriggerPressDownHandler, IGlobalTriggerPressDownHandler {

    private Renderer renderer;
    private Rigidbody rb;

  // Use this for initialization
  void Start () {
       renderer = GetComponent<Renderer>();
       rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
  }


    public void OnPointerTriggerPressDown(XREventData eventData) {
        XRHand pointingHand = eventData.hand;
        Color newCubeColor;
        if (pointingHand == XRHand.Left) newCubeColor = Color.blue;
        else newCubeColor = Color.red;
        renderer.material.color = newCubeColor;


    Vector3 controlPos = eventData.module.transform.position;
    Debug.Log(controlPos);
    Debug.Log(gameObject.transform.position);
    Vector3 direction = gameObject.transform.position - controlPos;
    Vector3 nor_dir = Vector3.Normalize(direction);
    Debug.Log("2333");
    rb.AddForce(nor_dir * 800);
    Debug.Log("1111");



  }

  public void OnGlobalTriggerPressDown(XREventData eventData) {
        Debug.Log("OnGlobalTriggerPressDown");
    }
}
