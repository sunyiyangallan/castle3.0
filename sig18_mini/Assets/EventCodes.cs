using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCodes {
  //IMPORTANT NOTE: THERE MAY BE A MAXIMUM NUMBER OF EVENT CODES. 200/201 DID NOT WORK.
  public const byte AvatarInstantiate = 10;
  public const byte AvatarDelete = 11;

  public const byte ObjectInstantiate = 12;
  public const byte ObjectAvatarInstantiate = 24;
  public const byte ObjectInstantiateParallelOrGhostOrWaypoint = 13;
  public const byte ObjectDelete = 14;

  public const byte SocialCircleInstantiate = 15;
  public const byte SocialCircleDelete = 16;
  public const byte SocialCircleJoin = 17;
  public const byte SocialCircleLeave = 18;

  public const byte ObjectSelect = 19;
  public const byte ObjectDeselect = 20;

  public const byte WaypointInstantiate = 21;
  public const byte WaypointDelete = 22;

  public const byte ObjectParent = 23;

  public const byte SendMatchingInfo = 30;


}
