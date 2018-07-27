
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using Assets.Utilities;

public class ConnectionManager : MonoBehaviour {

  private const string photonVersion = "0.0.1";
  private const string photonRoom = "SpaceTime";

  private static ConnectionManager instance;

  public static Dictionary<int[], int> objectsPhotonRecords = new Dictionary<int[], int>();

  public static ulong LocalOculusID { get; private set; }
  public static int PlayerCount { get; private set; }

  public string photonRoomOverride = "";

  public virtual void Awake() {
    if (instance) {
      Debug.LogError("There can be only one ConnectionManager!");
      DestroyImmediate(this);
    } else {
      instance = this;
    }
  }

  // Use this for initialization
  void Start() {
    PhotonNetwork.ConnectUsingSettings(photonVersion);
  }

  void OnJoinedLobby() {
    Debug.Log("Joined Lobby. Joining room...");
    RoomOptions roomOptions = new RoomOptions { IsVisible = false, MaxPlayers = 4, CleanupCacheOnLeave = false, };
    string roomName = photonRoomOverride.Equals("") ? photonRoom : photonRoomOverride;
    PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
  }


  void OnJoinedRoom() {
    Debug.Log("Joined Room.");
    PlayerCount = PhotonNetwork.playerList.Length;
    SpacetimeAvatarManager.InstantiateAvatar();
  }

  void OnLeftRoom() {
    Debug.Log("player leave room");
    PlayerCount = PhotonNetwork.playerList.Length;
    SpacetimeAvatarManager.DeleteAvatar(PhotonNetwork.player.ID);
  }
}
