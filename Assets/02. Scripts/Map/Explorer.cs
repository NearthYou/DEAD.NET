using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Hexamap;

public class Explorer : MonoBehaviour
{
    float lifeTime;
    public Tile curTile;
    public Tile targetTile;
    private bool goToMap;
    private bool isIdle;
    List<Coords> movePath;
    private int fogRevealerIndex = -1;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay1 = new WaitForSeconds(1.5f);

    public void Set(Tile tile)
    {
        lifeTime = 1f;
        curTile = tile;
    }

    public void Targeting(Tile tile)
    {
        targetTile = tile;
        GetComponentInChildren<MeshRenderer>().material.DOFade(100, 1f);
    }

    public IEnumerator Move(int walkCount = 2)
    {
        Tile nextTile;
        Vector3 targetPos;

        if (curTile != targetTile)
            movePath = AStar.FindPath(curTile.Coords, targetTile.Coords);

        if (lifeTime > 0)
        {
            if (movePath.Count < walkCount)
            {
                nextTile = MapController.instance.GetTileFromCoords(targetTile.Coords);
                targetPos = ((GameObject)nextTile.GameEntity).transform.position;
                targetPos.y += 0.5f;
                
                gameObject.transform.DOMove(targetPos, 0.5f);
                yield return delay05;
                curTile = nextTile;
            }
            else if (curTile != targetTile)
            {
                for (int i = 0; i < walkCount; i++)
                {
                    nextTile = MapController.instance.GetTileFromCoords(movePath[i]);
                    targetPos = ((GameObject)nextTile.GameEntity).transform.position;
                    targetPos.y += 0.5f;
                    
                    gameObject.transform.DOMove(targetPos, 0.5f);
                    yield return delay05;
                    curTile = nextTile;
                }
            }

            if (curTile == targetTile)
            {
                var fogRevealer = new FischlWorks_FogWar.csFogWar.FogRevealer(gameObject.transform, 3, false);
                FischlWorks_FogWar.csFogWar.instance.AddFogRevealer(fogRevealer);
                fogRevealerIndex = FischlWorks_FogWar.csFogWar.instance._FogRevealers.Count - 1;
                
                var sightTiles = MapController.instance.GetSightTiles(curTile);
                foreach (var tile in sightTiles)
                {
                    var tileBase = ((GameObject)tile.GameEntity).GetComponent<TileBase>();
                    if (tileBase != null)
                    {
                        tileBase.SetPlayerSight(true);
                    }
                }
                
                lifeTime -= 1;
            }
        }
        else
        {
            isIdle = true;
            StartCoroutine(ExplorerEffect());
        }
        movePath.Clear();
    }

    public IEnumerator ExplorerEffect()
    {
        yield return new WaitUntil(()=> goToMap == true);
        
        var sightTiles = App.instance.GetMapManager().mapController.GetSightTiles(curTile);
        foreach (var tile in sightTiles)
        {
            var tileBase = ((GameObject)tile.GameEntity).GetComponent<TileBase>();
            if (tileBase != null)
            {
                tileBase.SetPlayerSight(true);
            }
        }
        
        App.instance.GetMapManager().mapController.RemoveExplorer(this);
        
        if (fogRevealerIndex >= 0 && fogRevealerIndex < FischlWorks_FogWar.csFogWar.instance._FogRevealers.Count)
        {
            FischlWorks_FogWar.csFogWar.instance._FogRevealers[fogRevealerIndex].sightRange = 0;
        }

        goToMap = false;
        isIdle = false;
        yield return delay1;
        
        if (fogRevealerIndex >= 0)
        {
            FischlWorks_FogWar.csFogWar.instance.RemoveFogRevealer(fogRevealerIndex);
        }
        
        Destroy(gameObject);
    }
    
    public void Invocation()
    {
        goToMap = true;
    }

    public bool GetIsIdle()
    {
        return isIdle;
    }
}