using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
using FRL.IO;

public class SpacetimeObjectManager : MonoBehaviour {

  public enum ParentMode { Local, Global }

  private static SpacetimeObjectManager instance;

  private static int instantiateCallbackID = 0, deleteCallbackID = 0, parentCallbackID = 0;
  private static Dictionary<int, Action<SpacetimeObject>> instantiateCallbacks = new Dictionary<int, Action<SpacetimeObject>>();
  private static Dictionary<int, Action> deleteCallbacks = new Dictionary<int, Action>();
  private static Dictionary<int, Action<SpacetimeObject, Transform>> parentCallbacks = new Dictionary<int, Action<SpacetimeObject, Transform>>();

  public static void Create(string name, SpacetimeType type, Action<SpacetimeObject> callback = null) {
    int playerID = PhotonNetwork.player.ID;
    int viewID = PhotonNetwork.AllocateViewID();
    int parentID = -1;

    int eventID = -1;
    if (callback != null) {
      eventID = instantiateCallbackID++;
      instantiateCallbacks.Add(eventID, callback);
    }
    RaiseObjectInstantiateEvent(eventID, name, Vector3.zero, Quaternion.identity, Vector3.one, playerID, viewID, parentID, type);
  }

  public static void CreateLocal(string name, Vector3 position, Quaternion rotation, Vector3 scale, SpacetimeType type, Transform parent = null, Action<SpacetimeObject> callback = null) {
    Create(name, position, rotation, scale, type, parent, ParentMode.Local, callback);
  }

  public static void CreateGlobal(string name, Vector3 position, Quaternion rotation, Vector3 scale, SpacetimeType type, Transform parent = null, Action<SpacetimeObject> callback = null) {
    Create(name, position, rotation, scale, type, parent, ParentMode.Global, callback);
  }

  public static void Create(string name, Vector3 position, Quaternion rotation, Vector3 scale, SpacetimeType type, Transform parent = null, ParentMode mode = ParentMode.Global, Action<SpacetimeObject> callback = null) {
    int playerID = PhotonNetwork.player.ID;
    int viewID = PhotonNetwork.AllocateViewID();
    int parentID = -1;
    PhotonView parentView = null;
    if (parent) {
      parentView = parent.GetComponent<PhotonView>();
      if (parentView) parentID = parentView.viewID;
      if (mode == ParentMode.Global) {
        position = parent.InverseTransformPoint(position);
        rotation = Quaternion.Inverse(parent.rotation) * rotation;
        scale = new Vector3(scale.x / parent.lossyScale.x, scale.y / parent.lossyScale.y, scale.z / parent.lossyScale.z);
      }
    }

    int eventID = -1;
    if (callback != null) {
      eventID = instantiateCallbackID++;
      instantiateCallbacks.Add(eventID, callback);
    }
  }


  public static void Delete(int viewID, Action callback = null) {
    int eventID = -1;
    if (callback != null) {
      eventID = deleteCallbackID++;
      deleteCallbacks.Add(eventID, callback);
    }
    RaiseObjectDeleteEvent(eventID, PhotonNetwork.player.ID, viewID);
  }

  private static void RaiseObjectInstantiateEvent(int eventID, string name, Vector3 position, Quaternion rotation, Vector3 scale, int playerID, int viewID, int parentID, SpacetimeType type) {
    ObjectInstantiateEventData data = new ObjectInstantiateEventData(eventID, name, position, rotation, scale, playerID, viewID, parentID, type);
    byte[] byteData = ObjectByteConverter.ObjectToByteArray(data);
    bool raised = PhotonNetwork.RaiseEvent(EventCodes.ObjectInstantiate, byteData, true,
      new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });
  }

  private static void RaiseAvatarInstantiateEvent(int eventID, string name, Vector3 position, Quaternion rotation, Vector3 scale, int playerID, int viewID, int leftHandID, int rightHandID, int parentID, SpacetimeType type) {
    AvatarInstantiateEventData data = new AvatarInstantiateEventData(eventID, name, position, rotation, scale, playerID, viewID, leftHandID, rightHandID, parentID, type);
    byte[] byteData = ObjectByteConverter.ObjectToByteArray(data);
    bool raised = PhotonNetwork.RaiseEvent(EventCodes.ObjectAvatarInstantiate, byteData, true,
      new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });
  }

  private static void RaiseObjectDeleteEvent(int eventID, int playerID, int viewID) {
    ObjectDeleteEventData data = new ObjectDeleteEventData(eventID, playerID, viewID);
    byte[] byteData = ObjectByteConverter.ObjectToByteArray(data);
    bool raised = PhotonNetwork.RaiseEvent(EventCodes.ObjectDelete, byteData, true,
      new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });
  }

  private void Awake() {
    if (instance) {
      Debug.Log("There can be only one SpacetimeObjectManager!");
      DestroyImmediate(this);
    } else {
      instance = this;
    }
  }

  public void OnEnable() {
    PhotonNetwork.OnEventCall += OnEvent;
  }

  public void OnDisable() {
    PhotonNetwork.OnEventCall -= OnEvent;
  }

  private void OnObjectInstantiateEvent(object content, int senderID) {
    ObjectInstantiateEventData data = ObjectByteConverter.FromByteArray<ObjectInstantiateEventData>((byte[])content);

    SpacetimeObject stObject = null;
    switch (data.Type) {
      case SpacetimeType.Resource:
        stObject = GameObject.Instantiate<SpacetimeObject>(stObject);
        break;
    }

    PhotonView view = stObject.GetComponent<PhotonView>();
    if (!view) view = stObject.gameObject.AddComponent<PhotonView>();
    view.synchronization = ViewSynchronization.ReliableDeltaCompressed;

    stObject.Initialize(data.Name, data.Position, data.Rotation, data.Scale, data.PlayerID, data.ViewID, data.ParentID, data.Type);

    if (data.PlayerID == PhotonNetwork.player.ID && instantiateCallbacks.ContainsKey(data.EventID)) {
      Action<SpacetimeObject> callback = instantiateCallbacks[data.EventID];
      instantiateCallbacks.Remove(data.EventID);
        
      callback(stObject);
    }
  }

  //private void OnObjectAvatarInstantiateEvent(object content, int senderID) {
  //  AvatarInstantiateEventData data = ObjectByteConverter.FromByteArray<AvatarInstantiateEventData>((byte[])content);
  //  SpacetimeObject stObject = GameObject.Instantiate<AvatarProxyObject>(avatarProxyPrefab);

  //  PhotonView view = stObject.GetComponent<PhotonView>();
  //  if (!view)  view = stObject.gameObject.AddComponent<PhotonView>();
  //  view.synchronization = ViewSynchronization.Off;

  //  stObject.Initialize(data.Name, data.Position, data.Rotation, data.Scale, data.PlayerID, data.ViewID, data.ParentID, data.Type);
  //  // risky condition
  //  (stObject as AvatarProxyObject).thisAvatar.SetIsParallelTo((stObject as AvatarProxyObject).Target.GetComponent<SpacetimeAvatar>().ViewID, stObject.ViewID);

  //  ((AvatarProxyObject)stObject).InitializeHands(data.LeftHandID, data.RightHandID);

  //  if (data.PlayerID == PhotonNetwork.player.ID && instantiateCallbacks.ContainsKey(data.EventID)) {
  //    Action<SpacetimeObject> callback = instantiateCallbacks[data.EventID];
  //    instantiateCallbacks.Remove(data.EventID);
  //  }
  //}

  private void OnObjectDeleteEvent(object content, int senderID) {
    ObjectDeleteEventData data = ObjectByteConverter.FromByteArray<ObjectDeleteEventData>((byte[])content);
    SpacetimeObject sto;

    if (sto = SpacetimeObject.Get(data.ViewID)) {
      sto.Delete();
    }

    if (data.PlayerID == PhotonNetwork.player.ID && deleteCallbacks.ContainsKey(data.EventID)) {
      Action callback = deleteCallbacks[data.EventID];
      deleteCallbacks.Remove(data.EventID);
      callback();
    }
  }

  private void OnEvent(byte eventCode, object content, int senderID) {
    switch (eventCode) {
      case EventCodes.ObjectInstantiate:
        OnObjectInstantiateEvent(content, senderID);
        break;
      case EventCodes.ObjectDelete:
        OnObjectDeleteEvent(content, senderID);
        break;
      case EventCodes.ObjectAvatarInstantiate:
        //OnObjectAvatarInstantiateEvent(content, senderID);
        break;
    }
  }

  [Serializable]
  private class ObjectInstantiateEventData {

    public int EventID { get; private set; }
    public int PlayerID { get; private set; }
    public int ViewID { get; private set; }
    public string Name { get; private set; }
    public int ParentID { get; private set; }

    public SpacetimeType Type {
      get { return (SpacetimeType)stType; }
      set { stType = (int)value; }
    }

    private int stType;
    private float posX, posY, posZ;
    private float rotX, rotY, rotZ, rotW;
    private float scaleX, scaleY, scaleZ;

    public Vector3 Position {
      get { return new Vector3(posX, posY, posZ); }
      private set { posX = value.x; posY = value.y; posZ = value.z; }
    }

    public Quaternion Rotation {
      get { return new Quaternion(rotX, rotY, rotZ, rotW); }
      private set { rotX = value.x; rotY = value.y; rotZ = value.z; rotW = value.w; }
    }

    public Vector3 Scale {
      get { return new Vector3(scaleX, scaleY, scaleZ); }
      private set { scaleX = value.x; scaleY = value.y; scaleZ = value.z; }
    }

    public ObjectInstantiateEventData(int eventID, string name, Vector3 position, Quaternion rotation, Vector3 scale, int playerID, int viewID, int parentID, SpacetimeType type) {
      this.EventID = eventID;
      this.PlayerID = playerID;
      this.ViewID = viewID;
      this.Name = name;
      this.Position = position;
      this.Rotation = rotation;
      this.Scale = scale;
      this.ParentID = parentID;
      this.Type = type;
    }
  }

  [Serializable]
  private class AvatarInstantiateEventData {

    public int EventID { get; private set; }
    public int PlayerID { get; private set; }
    public int ViewID { get; private set; }
    public int LeftHandID { get; private set; }
    public int RightHandID { get; private set; }
    public string Name { get; private set; }
    public int ParentID { get; private set; }

    public SpacetimeType Type {
      get { return (SpacetimeType)stType; }
      set { stType = (int)value; }
    }

    private int stType;
    private float posX, posY, posZ;
    private float rotX, rotY, rotZ, rotW;
    private float scaleX, scaleY, scaleZ;

    public Vector3 Position {
      get { return new Vector3(posX, posY, posZ); }
      private set { posX = value.x; posY = value.y; posZ = value.z; }
    }

    public Quaternion Rotation {
      get { return new Quaternion(rotX, rotY, rotZ, rotW); }
      private set { rotX = value.x; rotY = value.y; rotZ = value.z; rotW = value.w; }
    }

    public Vector3 Scale {
      get { return new Vector3(scaleX, scaleY, scaleZ); }
      private set { scaleX = value.x; scaleY = value.y; scaleZ = value.z; }
    }


    public AvatarInstantiateEventData(int eventID, string name, Vector3 position, Quaternion rotation, Vector3 scale, int playerID, int viewID, int leftID, int rightID, int parentID, SpacetimeType type) {
      this.EventID = eventID;
      this.PlayerID = playerID;
      this.ViewID = viewID;
      this.Name = name;
      this.Position = position;
      this.Rotation = rotation;
      this.Scale = scale;
      this.ParentID = parentID;
      this.LeftHandID = leftID;
      this.RightHandID = rightID;
      this.Type = type;
    }
  }

  [Serializable]
  private class ObjectDeleteEventData {

    public int EventID { get; private set; }
    public int PlayerID { get; private set; }
    public int ViewID { get; private set; }

    public ObjectDeleteEventData(int eventID, int playerID, int viewID) {
      this.EventID = eventID;
      this.PlayerID = playerID;
      this.ViewID = viewID;
    }
  }
}
