using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FRL.IO;

[RequireComponent(typeof(Receiver))]
public class SpacetimeHand : SpacetimeObject, IGlobalTriggerPressSetHandler {

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Static / Global
  ////////////////////////////////////////////////////////////////////////////////

  public enum Hand { Left, Right }
  public enum Mode { Grab, Comment }

  private const float CONTAINER_SPAWN_TIME = 0.2f;
  public const float CONTAINER_OFFSET = 0.1f;


  ////////////////////////////////////////////////////////////////////////////////
  ////////// Instance
  ////////////////////////////////////////////////////////////////////////////////

  //FRL.XR Receiver for global input.
  public Receiver receiver { get; private set; }

  private List<SpacetimeObject> currentHeldObjects = new List<SpacetimeObject>();
  private List<SpacetimeObject> currentCollidedObjects = new List<SpacetimeObject>();

  public SpacetimeHand otherLocalHand;
  private float triggerHeldTime = 0f;

  public bool ThumbstickTouched { get; private set; }
  public bool PingButtonTouched { get; private set; }
  public bool IsTriggerHeld { get; private set; }


  public Vector2 ThumbstickAxis { get; private set; }

  public Action OnGrab;
  public Action OnGrabRelease;

  [HideInInspector]
  public Hand hand = Hand.Left;

  [HideInInspector]
  public Mode mode = Mode.Grab;

  public bool IsHolding { get { return currentHeldObjects.Count > 0; } }
  public bool IsCollided { get { return currentCollidedObjects.Count > 0; } }

  public SpacetimeObject HeldObject {
    get { return IsHolding ? currentHeldObjects[0] : null; }
  }

  public List<SpacetimeObject> HeldObjects {
    get { return currentHeldObjects; }
  }


  ////////////////////////////////////////////////////////////////////////////////
  ////////// Unity Functions
  ////////////////////////////////////////////////////////////////////////////////

  protected override void Awake() {
    type = SpacetimeType.Hand;
    view = GetComponent<PhotonView>();
    receiver = GetComponent<Receiver>();
  }

  private void DoGrab(int viewID) { view.RPC("Grab", PhotonTargets.AllViaServer, viewID); }

  public void DoRelease() {
    Release();
    view.RPC("Release", PhotonTargets.Others);
  }

  public void FindAndGrab() {

    var sto = SpacetimeObject.GetSmallestContaining(this.transform.position);

    //Check to see if the other hand is holding something.
    //if (otherLocalHand.IsHolding) {
    //  DoGrab(-1);
    //  return;
    //} else if (sto) {
      DoGrab(sto.ViewID);
    //}
  }

  [PunRPC]
  private void Grab(int viewID) {
    SpacetimeObject sto = SpacetimeObject.Get(viewID);
        if (sto)
        {
            TryPair(sto);
        }
    //} else if (otherLocalHand.IsHolding) {
    //  foreach (SpacetimeObject o in otherLocalHand.currentHeldObjects) {
    //    TryPair(o);
    //  }
    //}
  }

  public void TryPair(SpacetimeObject sto) {
    if (sto && sto.TryPairHand(this, this.CreatorID)) {
      if (!(this.currentHeldObjects.Contains(sto)))
        this.currentHeldObjects.Add(sto);

      if (OnGrab != null) OnGrab();
      if (sto.Primary != this && sto.Primary.OnGrab != null)
        sto.Primary.OnGrab();
    }
  }

  [PunRPC]
  public void Release() {
    List<SpacetimeObject> toReleaseObject = new List<SpacetimeObject>(this.currentHeldObjects);
    this.currentHeldObjects.Clear();
    foreach (SpacetimeObject sto in toReleaseObject) {
      if (sto.TryReleaseHand(this)) {
        if (sto.Primary != null && sto.Primary != this && sto.Primary.OnGrab != null)
          sto.Primary.OnGrab();
      }
    }
    if (OnGrabRelease != null) OnGrabRelease();
  }



  //public void OnTriggerEnter(Collider other) {
  //  SpacetimeObject sto = other.GetComponent<SpacetimeObject>();
  //  //if (sto && sto.transform != WorldManager.currentWorld.transform && !currentCollidedObjects.Contains(sto)) currentCollidedObjects.Add(sto);
  //  SpacetimeColliderPart part = other.GetComponent<SpacetimeColliderPart>();
  //  if (part && !currentCollidedObjects.Contains(part.parent)) {
  //    currentCollidedObjects.Add(part.parent);
  //  }
  //}

  //public void OnTriggerExit(Collider other) {
  //  SpacetimeObject sto = other.GetComponent<SpacetimeObject>();
  //  if (sto) currentCollidedObjects.Remove(sto);
  //  SpacetimeColliderPart part = other.GetComponent<SpacetimeColliderPart>();
  //  if (part) {
  //    currentCollidedObjects.Remove(part.parent);
  //  }
  //}

  public void OnGlobalTriggerPressDown(XREventData eventData) {
    if (receiver.module == null) return;
    IsTriggerHeld = true;
    triggerHeldTime = 0f;
    FindAndGrab();
  }

  public void OnGlobalTriggerPress(XREventData eventData) {
    if (receiver.module == null) return;
  }

  public void OnGlobalTriggerPressUp(XREventData eventData) {
    if (!receiver.module) return;
    DoRelease();
    IsTriggerHeld = false;
  }
}
