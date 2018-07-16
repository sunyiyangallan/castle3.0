using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setActive : MonoBehaviour {

    public Material mat;

	// Use this for initialization
	void Start () {
        gameObject.transform.localScale -= new Vector3(.2f, .2f, .2f);
        GetComponent<Renderer>().material.color = mat.color;
    }

    // Update is called once per frame
    void Update () {
      if (transform.parent.tag == "Selected")
    {
      GetComponent<MeshRenderer>().enabled = true;
      gameObject.transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + 0.3f, transform.parent.position.z);
      transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
    else
    {
      GetComponent<MeshRenderer>().enabled = false;

    }
  }
  
}
