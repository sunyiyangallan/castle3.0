using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class IsSelected : MonoBehaviour , IPointerTriggerPressDownHandler , IGlobalTriggerPressDownHandler {

  public Renderer rend;

  void Start () {
    rend = GetComponent<Renderer>();

  }

  // Update is called once per frame
  void Update () {
    if (gameObject.tag == "Des")
    {
      //rend.material.color = Color.blue;
      GetComponent<MeshRenderer>().enabled = true;
      transform.GetChild(2).position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 0.02f, gameObject.transform.position.z);
      //transform.localScale += new Vector3(.2f*LowPolyAnimalPack.WanderScript.selectedAnimalsCount,0f,.2f*LowPolyAnimalPack.WanderScript.selectedAnimalsCount);
    }
    else
    {
      //rend.material.color = Color.cyan;
      GetComponent<MeshRenderer>().enabled = false;
          //  GameObject.Destroy(gameObject);
    }

  }



  public void OnPointerTriggerPressDown(XREventData eventData) {
    XRHand pointingHand = eventData.hand;
    GameObject[] dess = GameObject.FindGameObjectsWithTag("Des"); //clear all the other gameobjects that's tagged "Des"
    foreach (GameObject GO in dess) {
      GO.tag = "Untagged";
      
    }
      gameObject.tag = "Des";
      
    

    
  }

    void FlashLabel()
    {

        if (gameObject.tag == "Des")
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }
    }

    public void OnGlobalTriggerPressDown(XREventData eventData) {
    
  }
}
