using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Hexamap;

public class MapPathfindingManager : MonoBehaviour
{
    private MapController mapController;
    private MapTileManager tileManager;
    private GameObject arrow;
    private GameObject arrowPrefab;
    private MapSettings mapSettings;
    
    public void Initialize(MapController mapController, MapTileManager tileManager, GameObject arrowPrefab, MapSettings mapSettings)
    {
        this.mapController = mapController;
        this.tileManager = tileManager;
        this.arrowPrefab = arrowPrefab;
        this.mapSettings = mapSettings;
    }
    
    public bool PlayerCanMoveCheck()
    {
        return mapController.Player.MoveRange != 0;
    }
    
    public bool SelectPlayerMovePoint(TileController tileController)
    {
        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && mapController.Player.TileController.Model != tileController.Model
            && mapController.LandformCheck(tileController))
        {
            SavePlayerMovePath(tileController);
            return true;
        }
        else
            return false;
    }
    
    public void SelectTileForDisturbtor(TileController tileController)
    {
        if (mapController.LandformCheck(tileController) == false)
            return;

        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && mapController.Player.TileController.Model != tileController.Model)
        {
            foreach (var item in mapController.Player.TileController.Model.Neighbours.Where(
                         item => item.Value == tileController.Model))
            {
                InstallDistrubtor(tileController, item.Key);
            }
        }
    }
    
    public void SelectTileForExplorer(TileController tileController)
    {
        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && mapController.Player.TileController.Model != tileController.Model)
        {
            InstallExplorer(tileController);
        }
    }
    
    public void SavePlayerMovePath(TileController tileController)
    {
        mapController.SetTargetTileController(tileController);
        mapController.Player.UpdateMovePath(AStar.FindPath(mapController.Player.TileController.Model.Coords, tileController.Model.Coords));
        tileManager.DeselectAllBorderTiles();
    }
    
    public void DeletePlayerMovePath()
    {
        mapController.Player.UpdateMovePath(null);
        tileManager.DeselectAllBorderTiles();
    }
    
    public bool IsMovePathSaved()
    {
        return mapController.Player.MovePath != null;
    }
    
    public void InstallDistrubtor(TileController tileController, CompassPoint direction)
    {
        mapController.DroneManager?.InstallDistrubtor(tileController, direction);
    }
    
    public void InstallExplorer(TileController tileController)
    {
        mapController.DroneManager?.InstallExplorer(tileController);
    }
    
    public void MovePointerOn(Vector3 _pos)
    {
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, _pos, Quaternion.identity);
        }
        
        _pos.y += mapSettings.playerSpawnHeight;
        arrow.transform.position = _pos;
        
        arrow.SetActive(true);
        App.instance.GetSoundManager().PlaySFX("SFX_Map_Select_Complete");
    }
    
    public void MovePointerOff()
    {
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (arrow.activeInHierarchy)
        {
            arrow.SetActive(false);
            App.instance.GetSoundManager().PlaySFX("SFX_Map_Select_Cancel");
        }
    }
    
    public void OnlyMovePointerOff()
    {
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (arrow.activeInHierarchy)
        {
            arrow.SetActive(false);
        }
    }
    
    public bool CalculateDistanceToPlayer(Tile tile, int range)
    {
        var searchTiles = tileManager.GetTilesInRange(tile, range);
        return searchTiles.Exists(x => x == mapController.Player.TileController.Model);
    }
    
    public Distrubtor CalculateDistanceToDistrubtor(Tile tile, int range)
    {
        return mapController.DroneManager?.CalculateDistanceToDistrubtor(tile, range);
    }
    
    public bool CheckPlayersView(TileController tileController)
    {
        if (tileController == null || mapController.Player?.TileController == null)
            return false;

        if (mapController.Player.TileController == tileController)
            return true;

        var getTiles = tileManager.GetTilesInRange(mapController.Player.TileController.Model, 3);
        return getTiles.Contains(tileController.Model);
    }
}
