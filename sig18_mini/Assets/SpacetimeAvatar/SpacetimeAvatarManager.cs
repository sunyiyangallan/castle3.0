using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;

public class SpacetimeAvatarManager : MonoBehaviour {

  private static SpacetimeAvatarManager instance;
  public static SpacetimeAvatar LocalAvatar { get { return instance.localAvatar; } }

  public static void InstantiateAvatar() {
    int playerID = PhotonNetwork.player.ID;
    int viewID = PhotonNetwork.AllocateViewID();
    int leftHandID = PhotonNetwork.AllocateViewID();
    int rightHandID = PhotonNetwork.AllocateViewID();
    RaiseAvatarInstantiateEvent(playerID, viewID, leftHandID, rightHandID);
  }

  public static void DeleteAvatar(int playerID) {
    RaiseAvatarDeleteEvent(playerID);
  }

  private static void RaiseAvatarInstantiateEvent(int playerID, int viewID, int leftHandID, int rightHandID) {
    AvatarInstantiateEventData data = new AvatarInstantiateEventData(playerID, viewID, leftHandID, rightHandID);
    byte[] byteData = ObjectByteConverter.ObjectToByteArray(data);
    bool raised = PhotonNetwork.RaiseEvent(EventCodes.AvatarInstantiate, byteData, true,
      new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });
  }

  private static void RaiseAvatarDeleteEvent(int playerID) {
    AvatarDeleteEventData data = new AvatarDeleteEventData(playerID);
    byte[] byteData = ObjectByteConverter.ObjectToByteArray(data);
    bool raised = PhotonNetwork.RaiseEvent(EventCodes.AvatarDelete, byteData, true,
      new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache, Receivers = ReceiverGroup.All });
  }

  public SpacetimeAvatar localAvatar;
  public SpacetimeAvatar remoteAvatarPrefab;

  private void Awake() {
    if (instance) {
      Debug.LogError("There can only be one AvatarManager!");
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

  private void OnAvatarInstantiateEvent(object content, int senderID) {
    AvatarInstantiateEventData data = ObjectByteConverter.FromByteArray<AvatarInstantiateEventData>((byte[])content);

    if (senderID == PhotonNetwork.player.ID) {
      localAvatar.Initialize(data.PlayerID, data.ViewID, data.LeftHandID, data.RightHandID);
    } else {
      SpacetimeAvatar remoteAvatar = GameObject.Instantiate<SpacetimeAvatar>(remoteAvatarPrefab);
      remoteAvatar.Initialize(data.PlayerID, data.ViewID, data.LeftHandID, data.RightHandID);
    }
  }

  private void OnAvatarDeleteEvent(object content, int senderID) {
    AvatarDeleteEventData data = ObjectByteConverter.FromByteArray<AvatarDeleteEventData>((byte[])content);
    //Debug.Log(SpacetimeAvatar.avatars.ContainsKey(data.PlayerID));
    if (SpacetimeAvatar.avatars.ContainsKey(data.PlayerID)) {
      Destroy(SpacetimeAvatar.avatars[data.PlayerID].gameObject);
      SpacetimeAvatar.avatars.Remove(data.PlayerID);
    }
  }

  private void OnEvent(byte eventCode, object content, int senderID) {
    switch (eventCode) {
      case EventCodes.AvatarInstantiate:
        OnAvatarInstantiateEvent(content, senderID);
        break;
      case EventCodes.AvatarDelete:
        OnAvatarDeleteEvent(content, senderID);
        break;
    }
  }

  [Serializable]
  public class AvatarInstantiateEventData {

    public int PlayerID { get; private set; }
    public int ViewID { get; private set; }
    public int LeftHandID { get; private set; }
    public int RightHandID { get; private set; }

    public AvatarInstantiateEventData(int playerID, int viewID, int leftHandID, int rightHandID) {
      this.PlayerID = playerID;
      this.ViewID = viewID;
      this.LeftHandID = leftHandID;
      this.RightHandID = rightHandID;
    }
  }

  [Serializable]
  public class AvatarDeleteEventData {
    public int PlayerID { get; private set; }
    public AvatarDeleteEventData(int playerID) {
      this.PlayerID = playerID;
    }
  }
}
