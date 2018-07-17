using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class SelectScript : MonoBehaviour, IPointerTriggerPressDownHandler {

    public void OnPointerTriggerPressDown(XREventData eventData)
    {
        XRHand pointingHand = eventData.hand;
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Selected");
        // foreach (GameObject a in animals) {
        //     selectedAnimalsCount++;
        // }
        //<--- commented to allow for selecting multiple animals

        //if (pointingHand == XRHand.Left)
        GameObject cb = GameObject.FindGameObjectWithTag("Des");



        if (gameObject.tag == "Animal") {
            gameObject.tag = "Selected"; //allow an unselected animal to be selected
            Debug.Log("hit");
            cb.transform.localScale += new Vector3(0.1f, 0, 0.1f);

        } else {
            gameObject.tag = "Animal";
            cb.transform.localScale -= new Vector3(0.1f, 0, 0.1f);

            //Debug.Log("animal already selected"); //prevent duplicants
        }

    }

    // added start and update just to be able to disable it
    void Start()
    {
        // GetComponent<Renderer>().material.color = Color.black;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
