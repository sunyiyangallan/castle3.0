using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;
using UnityEngine.Events;

public class SpacetimeAvatar : SpacetimeObject {

  public static Dictionary<int, SpacetimeAvatar> avatars = new Dictionary<int, SpacetimeAvatar>();

  public static SpacetimeAvatar LocalAvatar {
    get {
      if (avatars.ContainsKey(PhotonNetwork.player.ID)) return avatars[PhotonNetwork.player.ID];
      else return null;
    }
  }

  public PhotonView avatarView;

  [HideInInspector]
  public SpacetimeHand leftHand;
  [HideInInspector]
  public SpacetimeHand rightHand;

  public SpacetimeHand handPrefab;
  public XRControllerModule leftHandModule;
  public XRControllerModule rightHandModule;

  public int PlayerID { get; private set; }

  protected override void OnEnable() {
    if (PlayerID != 0) avatars.Add(this.PlayerID, this);
  }

  protected override void OnDisable() {
    avatars.Remove(this.PlayerID);
  }

  protected override void Update() {
    base.Update();
    if (this.view.isMine) {
      this.transform.position = Camera.main.transform.position;
      this.transform.rotation = Camera.main.transform.rotation;
    }
  }

  public void Initialize(int playerID, int viewID,int leftHandID, int rightHandID) {
    type = SpacetimeType.Avatar;
    this.PlayerID = playerID;
        this.view.viewID = viewID;

    InstantiateHands(playerID, leftHandID, rightHandID);
    avatars.Add(playerID, this);
    SpacetimeObject.spacetimeObjects.Add(viewID, this);
  }


  public override bool TryPairHand(SpacetimeHand hand, int ownerID, bool action = true) {
    return false;
  }

  public override void SetColor(Color c) {
    this.CurrentColor = c;
  }

  public void InstantiateHands(int playerID, int leftHandID, int rightHandID) {
    leftHand = GameObject.Instantiate<SpacetimeHand>(handPrefab);
    leftHand.hand = SpacetimeHand.Hand.Left;
    rightHand = GameObject.Instantiate<SpacetimeHand>(handPrefab);
    rightHand.hand = SpacetimeHand.Hand.Right;

    leftHand.otherLocalHand = rightHand;
    rightHand.otherLocalHand = leftHand;

    PhotonView leftHandView = leftHand.GetComponent<PhotonView>();
    PhotonView rightHandView = rightHand.GetComponent<PhotonView>();
    leftHandView.viewID = leftHandID;
    rightHandView.viewID = rightHandID;

    if (playerID == PhotonNetwork.player.ID) {
      //Local.
      Receiver leftHandReceiver = leftHand.GetComponent<Receiver>();
      Receiver rightHandReceiver = rightHand.GetComponent<Receiver>();
      leftHandModule.System = XRSystem.CV1;
      rightHandModule.System = XRSystem.CV1;
      leftHandReceiver.module = leftHandModule;
      leftHandReceiver.bindToModule = true;

      rightHandReceiver.module = rightHandModule;
      rightHandReceiver.bindToModule = true;
    }

    leftHand.CreatorID = playerID;
    rightHand.CreatorID = playerID;

    leftHandView.TransferOwnership(playerID);
    rightHandView.TransferOwnership(playerID);
    leftHand.transform.SetParent(this.transform);
    rightHand.transform.SetParent(this.transform);
  }
}
