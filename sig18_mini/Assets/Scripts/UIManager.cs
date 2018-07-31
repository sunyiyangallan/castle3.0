using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FRL.IO;

public class UIManager : MonoBehaviour, IPunObservable, IPointerTriggerPressDownHandler, IPointerTriggerPressUpHandler
{

    public void OnPointerTriggerPressDown(XREventData eventData)
    {
        ColorRed();
    }
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

    [PunRPC]
    public void ColorRed()
    {
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

    void IPointerTriggerPressDownHandler.OnPointerTriggerPressDown(XREventData eventData)
    {
        throw new System.NotImplementedException();
    }

    void IPointerTriggerPressUpHandler.OnPointerTriggerPressUp(XREventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
