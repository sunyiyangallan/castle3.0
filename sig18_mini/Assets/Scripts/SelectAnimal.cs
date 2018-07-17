using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;


public class SelectAnimal : MonoBehaviour, IPointerTriggerPressDownHandler, IPointerTriggerPressUpHandler {

    public void OnPointerTriggerPressDown(XREventData eventData)
    {
        // GetComponent<Renderer>().material.color = Color.red;
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Selected");
        foreach (GameObject a in animals) {
            a.tag = "Animal";
        }
        GameObject[] go = GameObject.FindGameObjectsWithTag("Animal");

        //disable grabbable and enable selectscript
        foreach (GameObject g in go) {
            Debug.Log("changed to select");
            g.GetComponent<Grabbable>().enabled = false;
            g.GetComponent<Receiver>().enabled = false;
            g.GetComponent<SelectScript>().enabled = true;
            //g.GetComponent<CreateCube>().enabled = true;
        }
        GameObject terrain = GameObject.FindGameObjectWithTag("Terrain");
        terrain.GetComponent<CreateCube>().enabled = true;

        Debug.Log("Select00");
    }

    public void OnPointerTriggerPressUp(XREventData eventData)
    {
        //GetComponent<Renderer>().material.color = Color.black;
    }

    // Use this for initialization
    void Start()
    {
        // GetComponent<Renderer>().material.color = Color.black;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
