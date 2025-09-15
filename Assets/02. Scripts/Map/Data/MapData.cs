using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    [Tooltip("Resource spawn probability")] public int resourcePercent = 60;
    [Tooltip("Sight range")] public int fogSightRange = 4;
    [Tooltip("Movement distance")] public int playerMovementPoint = 2;
    [Tooltip("Zombie detection range")] public int zombieDetectionRange = 3;
    [Tooltip("Zombie count")] public int zombieCount = 80;
    [Tooltip("Durability")] public int durability = 100;
}