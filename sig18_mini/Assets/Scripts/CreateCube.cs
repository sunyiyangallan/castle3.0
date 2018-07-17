using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

public class CreateCube : MonoBehaviour, IPointerTriggerPressDownHandler {
    public GameObject prefab;
    private int selectedCount = 0;
    public XRControllerModule module;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
   if (module.xrEventData.worldPosition != null)
   {

     dir = module.transform.position - module.xrEventData.worldPosition;
     Debug.DrawLine(module.transform.position, module.xrEventData.worldPosition, Color.red, 0.1f, true);


   }
   */


        /* snowball
        if (module.xrEventData.worldPosition != null)
        {
          Debug.Log(module.xrEventData.worldPosition);
          //Debug.Log(module.xrEventData.worldPosition);
          sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
          sphere.transform.position = module.xrEventData.worldPosition;
          sphere.transform.localScale -= new Vector3(0.9f, 0.9f, 0.9f);
          Debug.Log("sphere");
          Destroy(sphere, 0.05f);
        }
        */

        // this creates the line for the user to know what they are pointing at
        /*
        if (module.xrEventData.currentRaycast != null)
        {
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

        */

    }

    //this selects the animals - allow for multiple animals to be selected 
    //and changes the size of the destination target cylinder
    public void OnPointerTriggerPressDown(XREventData eventData)
    {
        selectedCount = 0;
        var hit = new RaycastHit();
        XRHand pointingHand = eventData.hand;
        Vector3 controllerPos = eventData.module.transform.position;
        Debug.Log(controllerPos);
        /*
        Ray ray = Camera.main.ScreenPointToRay(eventData.worldPosition);
        if (Physics.Raycast(ray, out hit)) {
          Debug.Log("hit");
          Debug.Log(hit.transform.position);
          */
        Debug.Log(eventData.worldPosition);
        if (module.xrEventData.currentRaycast != null) {
            GameObject obj = Instantiate(prefab, new Vector3(eventData.worldPosition.x, eventData.worldPosition.y, eventData.worldPosition.z), Quaternion.identity) as GameObject;

            GameObject[] dess = GameObject.FindGameObjectsWithTag("Des"); //clear all the other gameobjects that's tagged "Des"
            foreach (GameObject GO in dess) {
                GO.tag = "Untagged";
                // GameObject.Destroy(GO);
            }
            obj.tag = "Des";
            //detects the number of animals selected
            GameObject[] sels = GameObject.FindGameObjectsWithTag("Selected");
            foreach (GameObject GO in sels) {
                selectedCount++;
            }
            //adjust the size of the target cylinder according to the number of selected animals
            obj.transform.localScale += new Vector3(0.1f * selectedCount, 0, 0.1f * selectedCount);

        }
    }


}



