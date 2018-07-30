using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using UnityEngine.UI;

public class GrabAnimal : MonoBehaviour, IPointerTriggerPressDownHandler, IPointerTriggerPressUpHandler, IPunObservable {

    Renderer objectRenderer;

   

    public void OnPointerTriggerPressDown(XREventData eventData)
    {
        //GetComponent<Renderer>().material.color = Color.red;
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Selected");
        foreach (GameObject a in animals) {
            a.tag = "Animal";
        }
        GameObject[] go = GameObject.FindGameObjectsWithTag("Animal");

        // disable selectscript and enable grabbabble
        foreach (GameObject g in go) {
            Debug.Log("Animal");
            g.GetComponent<Grabbable>().enabled = true;
            g.GetComponent<Receiver>().enabled = true;
            g.GetComponent<SelectScript>().enabled = false;
            //g.GetComponent<CreateCube>().enabled = false;
        }
        //disable the createcube script so that cubes won't be created in grab mode
        //GameObject terrain = GameObject.FindGameObjectWithTag("Terrain");
        //terrain.GetComponent<CreateCube>().enabled = false;

        //change the cube with tag des to unselected so that it won't be shown in grab mode
        //GameObject descube = GameObject.FindGameObjectWithTag("Des");
        //descube.tag = "Untagged";
<<<<<<< HEAD
        /*objectRenderer = GetComponent<Renderer>();
=======
       // objectRenderer = GetComponent<Renderer>();
>>>>>>> c6b1619de1b9bc071f8d4b2f749940299c921757
        Debug.Log("Grab");
        foreach (Button b in Canvas.FindObjectsOfType<Button>())
        {
            b.GetComponent<Image>().color = Color.white;
        }
        GetComponent<Image>().color = Color.red;
        */
        
        
    }

    public void OnPointerTriggerPressUp(XREventData eventData)
    {
        //GetComponent<Renderer>().material.color = Color.black;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        objectRenderer = GetComponent<Renderer>();
        
        foreach (Button b in Canvas.FindObjectsOfType<Button>())
        {
            b.GetComponent<Image>().color = Color.white;
        }
        GetComponent<Image>().color = Color.red;
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
