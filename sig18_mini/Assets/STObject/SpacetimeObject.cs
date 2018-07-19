using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
using FRL.IO;

public enum SpacetimeType {
  Resource, Poly, Container, Proxy, Avatar, AvatarProxy, Hand, Comment
}

[RequireComponent(typeof(PhotonView))]
public class SpacetimeObject : MonoBehaviour {

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Static
  ////////////////////////////////////////////////////////////////////////////////
  public const float PING_TIME = 1.027f;
  public const float MIN_SCALE = 0.00001f;
  public const float WAYPOINT_TRANSPARENCY = 0.2f;

  public const float WAYPOINT_DECAY_PERIOD = 1000f;

  public const float MAX_SCALE = float.MaxValue;

  public static Dictionary<int, SpacetimeObject> spacetimeObjects = new Dictionary<int, SpacetimeObject>();

  public static SpacetimeObject Get(int viewID) {
    return Get<SpacetimeObject>(viewID);
  }

  public static T Get<T>(int viewID) where T : SpacetimeObject {
    if (!spacetimeObjects.ContainsKey(viewID)) return default(T);
    SpacetimeObject sto = spacetimeObjects[viewID];
    if (sto is T) return (sto as T);
    else return default(T);
  }

  public static List<SpacetimeObject> GetAll() {
    return new List<SpacetimeObject>(spacetimeObjects.Values);
  }

  public static List<T> GetAll<T>() where T : SpacetimeObject {
    List<T> result = new List<T>();
    foreach (SpacetimeObject sto in spacetimeObjects.Values) {
      if (sto is T) result.Add(sto as T);
    }
    return result;
  }

  public static SpacetimeObject GetFirstContaining(Vector3 position) {
    return GetFirstContaining<SpacetimeObject>(position);
  }

  public static T GetFirstContaining<T>(Vector3 position) where T : SpacetimeObject {
    foreach (SpacetimeObject sto in spacetimeObjects.Values) {
      if (sto.Contains(position) && sto is T) return (T)sto;
    }
    return null;
  }

  public static SpacetimeObject GetSmallestContaining(Vector3 position) {
    return GetSmallestContaining<SpacetimeObject>(position);
  }

  public static T GetSmallestContaining<T>(Vector3 position, Transform except = null) where T : SpacetimeObject {
    T sto = default(T);
    float smallest = float.MaxValue;
    foreach (SpacetimeObject s in spacetimeObjects.Values) {
      if (s != null && s.transform != except && s.Contains(position) && s is T && s.transform.lossyScale.x < smallest && s.transform.lossyScale.magnitude > MIN_SCALE) {
        sto = (T)s;
        smallest = s.transform.lossyScale.x;
      }
    }
    return sto;
  }

  public static List<SpacetimeObject> GetWithinRange(Vector3 center, float radius) {
    List<SpacetimeObject> results = new List<SpacetimeObject>();
    foreach (SpacetimeObject s in spacetimeObjects.Values) {
      if (s != null && s.WithinRange(center, radius) && results.Contains(s) == false) results.Add(s);
    }
    return results;
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Instance
  ////////////////////////////////////////////////////////////////////////////////

  public string Name { get; protected set; }
  public int CreatorID { get; set; }

  public float lastManipulateTime { get; protected set; }

  public int ViewID {
    get {
      PhotonView photonView = GetComponent<PhotonView>();
      if (photonView) return photonView.viewID;
      else return -1;
    }
  }
  public bool IsInitialized { get; protected set; }
  public bool IsMoving { get; set; }

  public bool IsBeingManipulated { get { return Primary != null; } }
  public bool IsBeingPointed { get; set; }
  public bool IsBeingTwoHandManipulated { get { return Primary != null && Secondary != null; } }

  public bool IsBeingManipulatedBySelf {
    get {
      if (Primary != null || Secondary != null) {
        if (Primary && Primary.CreatorID == PhotonNetwork.player.ID)
          return true;
        if (Secondary && Secondary.CreatorID == PhotonNetwork.player.ID)
          return true;
        return false;
      } else {
        return false;
      }
    }
  }

  public bool IsBeingManipulatedByRemote {
    get {
      if (Primary != null) {
        if (Primary.CreatorID != PhotonNetwork.player.ID)
          return true;
        else
          return false;
      } else
        return false;
    }
  }

  public SpacetimeHand Primary { get; protected set; }
  public SpacetimeHand Secondary { get; protected set; }
  protected Vector3 primaryOffset;
  protected Vector3 primaryGrabPosition, secondaryGrabPosition;
  protected Vector3 midPointOffset;
  protected Quaternion primaryGrabRotation, secondaryGrabRotation;
  protected float savedDistance;
  protected Quaternion savedRotation;
  protected Vector3 savedScale;
  protected Transform previousParent;
  protected int primaryOwnerID;
  protected int secondaryOwnerID;

  public bool IsBeingDeleted { get; protected set; }

  public SpacetimeObject Parent { get; set; }
  public Vector3 LocalPosition { get; set; }
  public Quaternion LocalRotation { get; set; }
  public Vector3 LocalScale { get; set; }
  public Color CurrentColor { get; set; }


  public SpacetimeType type;
  public PhotonView view;

  public Action OnPaired;
  public Action OnCreate;
  public Action OnDelete;
  public Action OnReleased;
  public Action OnPing;
  public Action<Color> OnSetColor;

  public bool startsInScene = false;


  ////////////////////////////////////////////////////////////////////////////////
  ////////// Unity Functions
  ////////////////////////////////////////////////////////////////////////////////

  protected virtual void Awake() {
    view = GetComponent<PhotonView>();

  }

  protected virtual void Start() {
    if (startsInScene) {
      this.InitializeInScene();
    }
  }

  protected virtual void Update() {
    if (IsBeingManipulated) {
      Manipulate();
      lastManipulateTime = Time.time;
    }
  }

  protected virtual void OnEnable() {
    if (ViewID > 0 && spacetimeObjects.ContainsKey(ViewID) == false)
      spacetimeObjects.Add(ViewID, this);
  }

  protected virtual void OnDisable() {
    spacetimeObjects.Remove(this.ViewID);
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Initialize Functions
  ////////////////////////////////////////////////////////////////////////////////

  public virtual void InitializeInScene() {
    if (!view) view = GetComponent<PhotonView>();
    ConfigureBasicTransformAndMetaSync();
    this.Name = this.name;

    foreach (Collider c in GetComponentsInChildren<Collider>()) {
      Destroy(c);
    }

    foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>()) {
      MeshCollider c = filter.gameObject.AddComponent<MeshCollider>();
      c.sharedMesh = filter.sharedMesh;
      //c.convex = true;
    }

    bool boundsSet = false;
    Bounds bounds = new Bounds();
    foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
      if (boundsSet) bounds.Encapsulate(r.bounds);
      else {
        bounds = r.bounds;
        boundsSet = true;
      }

      Collider b = r.gameObject.GetComponent<Collider>();
    }

    IsInitialized = true;
    if (spacetimeObjects.ContainsKey(this.view.viewID) == false)
      spacetimeObjects.Add(this.view.viewID, this);

    this.lastManipulateTime = Time.time;
  }

  public virtual void Initialize(string name, Vector3 position, Quaternion rotation, Vector3 scale, int playerID, int viewID, int parentID, SpacetimeType type) {
    if (!view) view = GetComponent<PhotonView>();
    this.Name = name;
    view.viewID = viewID;
    view.TransferOwnership(playerID);

    CreatorID = playerID;

    //Transform parent = null;
    //if (parentID > -1) {
    //  PhotonView parentView = PhotonView.Find(parentID);
    //  if (parentView) parent = parentView.transform;
    //}
    //this.transform.SetParent(parent);
    ConfigureBasicTransformAndMetaSync();

    this.transform.localPosition = position;
    this.transform.localRotation = rotation;
    this.transform.localScale = scale;
    this.type = type;

    foreach (Collider c in GetComponentsInChildren<Collider>()) {
      Destroy(c);
    }

    foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>()) {
      if (filter.sharedMesh.vertexCount > 256) {
        MeshCollider c = filter.gameObject.AddComponent<MeshCollider>();
        c.sharedMesh = filter.sharedMesh;
        c.convex = true;
      } else {
        BoxCollider b = filter.gameObject.AddComponent<BoxCollider>();
        b.bounds.Encapsulate(filter.gameObject.GetComponent<Renderer>().bounds);
      }
    }

    bool boundsSet = false;
    Bounds bounds = new Bounds();
    foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
      if (boundsSet) bounds.Encapsulate(r.bounds);
      else {
        bounds = r.bounds;
        boundsSet = true;
      }
    }

    //Decrease scale by the size, so we end up with everything within a 1m box.
    float size = boundsSet ? bounds.extents.magnitude * 2f : 1f;
    if (size != 0.0f)
      this.transform.localScale /= size;

    IsInitialized = true;
    if (spacetimeObjects.ContainsKey(viewID) == false)
      spacetimeObjects.Add(viewID, this);

    this.lastManipulateTime = Time.time;
    StopSynchronization();
  }

  protected void ConfigureBasicTransformAndMetaSync() {
    PhotonTransformView transformView = this.GetComponent<PhotonTransformView>();
    if (!transformView) transformView = this.gameObject.AddComponent<PhotonTransformView>();
    transformView.m_PositionModel.SynchronizeEnabled = true;
    transformView.m_PositionModel.InterpolateOption = PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues;
    //transformView.m_PositionModel.InterpolateLerpSpeed;
    transformView.m_RotationModel.SynchronizeEnabled = true;
    transformView.m_ScaleModel.SynchronizeEnabled = true;
    transformView.m_ScaleModel.InterpolateOption = PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards;
    transformView.m_ScaleModel.InterpolateMoveTowardsSpeed = 0.3f;

    if (view.ObservedComponents == null) view.ObservedComponents = new List<Component>();
    if (!view.ObservedComponents.Contains(this)) view.ObservedComponents.Add(this);
    if (!view.ObservedComponents.Contains(transformView)) view.ObservedComponents.Add(transformView);
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Utility Functions
  ////////////////////////////////////////////////////////////////////////////////

  public virtual void SetColor(Color c) {
    //Debug.Log(this.name + " " + c);
    this.CurrentColor = c;
    foreach (Renderer r in GetComponentsInChildren<Renderer>())
      r.material.SetColor("_Color", c);
    if (OnSetColor != null) OnSetColor(this.CurrentColor);
  }

  public virtual bool Contains(Vector3 position) {
    foreach (Collider collider in GetComponentsInChildren<Collider>()) {
      if (collider != null && collider.bounds.Contains(position)) return true;
    }
    foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
      if (renderer is LineRenderer || renderer is TrailRenderer) continue;
      if (renderer != null && renderer.bounds.Contains(position)) return true;
    }
    return false;
  }

  public virtual bool WithinRange(Vector3 position, float range) {
    foreach (Collider collider in GetComponentsInChildren<Collider>()) {
      if (collider != null && (collider.bounds.ClosestPoint(position) - position).magnitude < range) return true;
    }
    foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
      if (renderer != null && (renderer.bounds.ClosestPoint(position) - position).magnitude < range) return true;
    }
    return false;
  }


  public virtual Bounds GetBounds() {
    Bounds bounds = new Bounds();
    bool hasSet = false;
    foreach (var r in this.GetComponentsInChildren<MeshRenderer>()) {
      if (!hasSet) {
        bounds = new Bounds(r.bounds.center, Vector3.one * r.bounds.size.magnitude);
        hasSet = true;
      } else {
        bounds.Encapsulate(new Bounds(r.bounds.center, Vector3.one * r.bounds.size.magnitude));
      }
    }
    return bounds;
  }

  public Bounds GetCloseBounds() {
    Bounds bounds = new Bounds();
    bool hasSet = false;
    foreach (var r in this.GetComponentsInChildren<Renderer>()) {
      if (!hasSet) {
        bounds = r.bounds;
        hasSet = true;
      } else {
        bounds.Encapsulate(r.bounds);
      }
    }
    return bounds;
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////// Manipulate Functions
  ////////////////////////////////////////////////////////////////////////////////

  private float GetManipulationScaleDifference() {
    if (!Primary || !Secondary) return 1f;
    return Vector3.Distance(Primary.transform.position, Secondary.transform.position) / savedDistance;
  }

  protected virtual void Manipulate() {
    if (Primary && Secondary) {
      //Scale.
      float diff = GetManipulationScaleDifference();
      Vector3 newScale = Vector3.LerpUnclamped(MIN_SCALE * Vector3.one, savedScale, diff);
      Vector3 n = savedScale;
      Vector3 offset = midPointOffset;

      this.transform.localScale = n;

      //Rotate
      Vector3 a = primaryGrabPosition - secondaryGrabPosition;
      Vector3 b = Primary.transform.position - Secondary.transform.position;
      Quaternion _savedRotaton = savedRotation;
      Quaternion rot = Quaternion.FromToRotation(a, b);

      //Midpoint rotation
      Vector3 midPoint = (Primary.transform.position + Secondary.transform.position) / 2f;

      this.transform.position = midPoint + rot * offset;
      this.transform.rotation = rot * _savedRotaton;
    } else if (Primary) {
      //Position
      Quaternion primaryRotation = Primary.transform.rotation;

      this.transform.position = Primary.transform.position + primaryRotation * primaryOffset;
      //Rotation
      Quaternion newRotation = primaryRotation * savedRotation;
      //this.transform.rotation = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);
      this.transform.rotation = newRotation;
    }
  }

  public virtual void UpdateOffsets(Vector2 axis) {
    if (Secondary == null) {
      primaryOffset += Vector3.forward * axis.y * Time.deltaTime;
      savedRotation = Quaternion.Euler(new Vector3(0f, 90f * axis.x * Time.deltaTime, 0f)) * savedRotation;
    } else {
      midPointOffset += Vector3.forward * axis.y * Time.deltaTime;
      savedRotation = Quaternion.Euler(new Vector3(0f, 90f * axis.x * Time.deltaTime, 0f)) * savedRotation;
    }
  }

  // if you are grabbing parallel objects, then you creat a new object out of it.

  public virtual bool TryPairHand(SpacetimeHand hand, int ownerID, bool action = true) {
    if (IsBeingDeleted) return false;
    if (PairHand(hand, ownerID) == false) return false;
    if (!Primary) StartSynchronization();
    if (action && OnPaired != null) OnPaired();
    return true;
  }

  public virtual bool TryReleaseHand(SpacetimeHand hand, bool sound = true) {
    int primaryOwnerID = -1;
    if (Primary && (hand == Primary))
      primaryOwnerID = hand.CreatorID;

    if (ReleaseHand(hand) == false) return false;
    if (Primary == null) {
      lastManipulateTime = Time.time;
    }
    if (Primary == null && primaryOwnerID == PhotonNetwork.player.ID) {
      StopSynchronization();
    }
    if (OnReleased != null && sound) OnReleased();
    return true;
  }

  public bool PairHand(SpacetimeHand hand, int ownerID) {
    if (!Primary) {
      //Register the primary hand.  
      UpdatePrimaryHand(hand, ownerID);
    } else if (!Secondary) {
      //Register the secondary hand, and record positions and scales.
      UpdatePrimaryHand(Primary, primaryOwnerID);
      UpdateSecondaryHand(hand, ownerID);
    } else {
      return false;
    }
    return true;
  }

  public bool ReleaseHand(SpacetimeHand hand) {
    if (hand == Primary) {
      //Release primary hand, and move secondary hand to primary spot.
      UpdatePrimaryHand(Secondary, secondaryOwnerID);
      UpdateSecondaryHand(null, -1);
    } else if (hand == Secondary) {
      //Release secondary hand, and recalculate primary offset.
      UpdatePrimaryHand(Primary, primaryOwnerID);
      UpdateSecondaryHand(null, -1);
    } else {
      return false;
    }
    return true;
  }

  public virtual bool TrySwitchHand(SpacetimeHand hand, SpacetimeObject sto) {
    return false;
  }

  protected virtual void UpdatePrimaryHand(SpacetimeHand hand, int ownerID) {
    Primary = hand;
    if (Primary == null) return;
    primaryOwnerID = ownerID;
    view.TransferOwnership(ownerID);
    primaryOffset = this.transform.position - Primary.transform.position;
    primaryGrabPosition = Primary.transform.position;
    primaryGrabRotation = Primary.transform.rotation;
    primaryOffset = Quaternion.Inverse(primaryGrabRotation) * primaryOffset;
    savedRotation = Quaternion.Inverse(primaryGrabRotation) * this.transform.rotation;
  }

  protected virtual void UpdateSecondaryHand(SpacetimeHand hand, int ownerID) {
    Secondary = hand;
    if (hand == null) return;

    secondaryOwnerID = ownerID;
    secondaryGrabPosition = Secondary.transform.position;
    secondaryGrabRotation = Secondary.transform.rotation;
    Vector3 midPoint = (Primary.transform.position + Secondary.transform.position) / 2f;
    midPointOffset = this.transform.position - midPoint;
    savedScale = this.transform.localScale;
    savedRotation = this.transform.rotation;
    savedDistance = Vector3.Distance(Primary.transform.position, Secondary.transform.position);
  }

  public virtual void Delete() {
    Destroy(gameObject);
    PhotonNetwork.UnAllocateViewID(ViewID);
  }

  public void TurnOffColliders() {
    var allColliders = this.transform.GetComponentsInChildren<Collider>();
    foreach (var c in allColliders) {
      c.enabled = false;
    }
  }

  public void RemoveColliders() {
    var allColliders = this.transform.GetComponentsInChildren<Collider>();
    foreach (var c in allColliders) {
      Destroy(c);
    }
  }
  public void TurnOnColliders() {
    if (!this) return;
    var allColliders = this.transform.GetComponentsInChildren<Collider>();
  }

  public void TurnOffRenderer() {
    foreach (var r in GetComponentsInChildren<Renderer>())
      r.enabled = false;
  }

  public void TurnOnRenderer() {
    foreach (var r in GetComponentsInChildren<Renderer>())
      r.enabled = true;
  }


  public void StartSynchronization() {
    if (view != null && SpacetimeAvatar.LocalAvatar != null) {
      SpacetimeAvatar.LocalAvatar.GetComponent<PhotonView>().RPC("StartSynchronization", PhotonTargets.AllViaServer, view.viewID);
    }
  }

  public void StopSynchronization() {
    if (view != null && SpacetimeAvatar.LocalAvatar != null) {
      SpacetimeAvatar.LocalAvatar.GetComponent<PhotonView>().RPC("StopSynchronization", PhotonTargets.AllViaServer, view.viewID);
    }
  }

  protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
    if (stream.isWriting) {
      byte[] packet = GetStatusAsPacket();
      stream.SendNext(packet);
    } else if (stream.isReading) {
      byte[] packet = stream.ReceiveNext() as byte[];
      SetStatusFromPacket(packet);
    }
  }

  protected virtual byte[] GetStatusAsPacket() {
    SpacetimeObjectStatus status = new SpacetimeObjectStatus(this);
    return ObjectByteConverter.ObjectToByteArray(status);
  }

  protected virtual void SetStatusFromPacket(byte[] packet) {
    SpacetimeObjectStatus status = ObjectByteConverter.FromByteArray<SpacetimeObjectStatus>(packet);
    SetColor(status.CurrentColor);
  }

  [Serializable]
  protected class SpacetimeObjectStatus {

    public SpacetimeType Type {
      get { return (SpacetimeType)typeAsInt; }
      set { typeAsInt = (int)value; }
    }

    public int ViewID { get; private set; }
    public int ParentID { get; private set; }

    public Vector3 LocalPosition {
      get { return new Vector3(pX, pY, pz); }
      set { pX = value.x; pY = value.y; pz = value.z; }
    }

    public Quaternion LocalRotation {
      get { return new Quaternion(rX, rY, rZ, rW); }
      set { rX = value.x; rY = value.y; rZ = value.z; rW = value.w; }
    }

    public Vector3 LocalScale {
      get { return new Vector3(sX, sY, sZ); }
      set { sX = value.x; sY = value.y; sZ = value.z; }
    }

    public Color CurrentColor {
      get { return new Color(r, g, b, a); }
      set { r = value.r; g = value.g; b = value.b; a = value.a; }
    }

    private int typeAsInt;
    private float pX, pY, pz, rX, rY, rZ, rW, sX, sY, sZ;
    private float r, g, b, a;

    public SpacetimeObjectStatus(SpacetimeObject sto) {
      this.Type = sto.type;
      this.ViewID = sto.ViewID;
      this.ParentID = (sto.Parent != null ? sto.Parent.ViewID : -1);

      this.LocalPosition = sto.LocalPosition;
      this.LocalRotation = sto.LocalRotation;
      this.LocalScale = sto.LocalScale;
      this.CurrentColor = sto.CurrentColor;
    }
  }
}
