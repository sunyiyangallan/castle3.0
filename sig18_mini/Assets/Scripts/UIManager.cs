using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IPunObservable {
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(Color.red);
        }
        else
        {
            Color c = (Color)stream.ReceiveNext();
        }
        foreach (Button b in Canvas.FindObjectsOfType<Button>())
        {
            b.GetComponent<Image>().color = Color.white;
        }
        GetComponent<Image>().color = Color.red;

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
