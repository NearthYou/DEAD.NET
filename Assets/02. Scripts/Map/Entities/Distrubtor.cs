using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hexamap;

[System.Serializable]
public class CompassPointObjects : SerializableDictionary<CompassPoint, GameObject> { };

public class Distrubtor : MonoBehaviour
{
    float lifeTime;
    public Tile currentTile;
    CompassPoint direction;

    [SerializeField] CompassPointObjects objects;

    public void Set(Tile tile, CompassPoint cp)
    {
        //App.instance.GetDataManager().gameData.TryGetValue("DISRUBTOR_LIFETIME", out GameData time);
        lifeTime = 4f;
        currentTile = tile;
        direction = cp;

        //GetComponentInChildren<MeshRenderer>().material.DOFade(100, 1f);
    }

    public void Move()
    {
        currentTile.Neighbours.TryGetValue(direction, out Tile nextTile);

        if (lifeTime > 0)
        {
            if (nextTile.Landform.GetType().Name == "LandformWorldLimit")
            {
                lifeTime -= 1;
            }
            else
            {
                transform.DOMove(((GameObject)nextTile.GameEntity).transform.position + Vector3.up, 0.5f);
                currentTile = nextTile;
                lifeTime -= 1;
            }
        }
        else
        {
            App.instance.GetMapManager().mapController.RemoveDistrubtor(this);
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    public GameObject GetDirectionObject(CompassPoint compass)
    {
        objects.TryGetValue(compass, out GameObject value);
        return value;
    }

    public void DirectionObjectOff()
    {
        foreach (var item in objects)
        {
            item.Value.SetActive(false);
        }
    }
}
