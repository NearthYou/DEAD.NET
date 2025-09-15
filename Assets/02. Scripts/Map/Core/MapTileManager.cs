using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Hexamap;

public class MapTileManager : MonoBehaviour
{
    private List<TileController> selectedTiles = new List<TileController>();
    private List<TileController> droneSelectedTiles = new List<TileController>();
    private List<TileController> pathTiles = new List<TileController>();
    
    private List<Tile> _cachedAllTiles;
    private bool _tilesCacheDirty = true;
    
    private HexamapController hexaMap;
    private MapController mapController;
    
    public void Initialize(HexamapController hexaMap, MapController mapController)
    {
        this.hexaMap = hexaMap;
        this.mapController = mapController;
    }
    
    public List<Tile> GetAllTiles()
    {
        if (_tilesCacheDirty || _cachedAllTiles == null)
        {
            _cachedAllTiles = hexaMap.Map.Tiles.Where(x => ((GameObject)x.GameEntity).CompareTag("Tile")).ToList();
            _tilesCacheDirty = false;
        }
        return _cachedAllTiles;
    }
    
    public Tile GetTileFromCoords(Coords coords)
    {
        return hexaMap.Map.GetTileFromCoords(coords);
    }
    
    public List<Tile> GetTilesInRange(Tile tile, int num)
    {
        return hexaMap.Map.GetTilesInRange(tile, num);
    }
    
    public TileController TileToTileController(Tile tile)
    {
        return ((GameObject)tile.GameEntity).GetComponent<TileController>();
    }
    
    public void AddSelectedTilesList(TileController tileController)
    {
        selectedTiles.Add(tileController);
    }
    
    public void AddToDroneSelectedTiles(TileController tileController)
    {
        droneSelectedTiles.Add(tileController);
    }
    
    public List<TileController> GetDroneSelectedTiles()
    {
        return droneSelectedTiles;
    }
    
    public void DeselectAllBorderTiles()
    {
        if (selectedTiles?.Count > 0)
        {
            ClearTiles(selectedTiles);
        }

        if (pathTiles?.Count > 0)
        {
            ClearTiles(pathTiles);
        }
    }
    
    public void DeselectAllTargetTiles()
    {
        if (droneSelectedTiles?.Count > 0)
        {
            for (int i = 0; i < droneSelectedTiles.Count; i++)
            {
                TileController tile = droneSelectedTiles[i];
                if (tile != null)
                {
                    DeselecTargetBorder(tile);
                }
            }
            droneSelectedTiles.Clear();
        }
    }
    
    public void SelectTargetBorder(TileController tileController)
    {
        tileController.GetComponent<Borders>().GetDisturbanceBorder().SetActive(true);
        droneSelectedTiles.Add(tileController);
    }
    
    public void DeselecTargetBorder(TileController tileController)
    {
        tileController.GetComponent<Borders>().OffTargetBorder();

        if (droneSelectedTiles.Contains(tileController))
            droneSelectedTiles.Remove(tileController);
    }
    
    public void DefaultMouseOverState(TileController tileController)
    {
        if (mapController.LandformCheck(tileController) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (tileController != null && !selectedTiles.Contains(tileController))
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }
    
    public void TilePathFinderSurroundings(TileController tileController)
    {
        var neighborTiles = hexaMap.Map.GetTilesInRange(mapController.Player.TileController.Model, mapController.Player.MoveRange);

        var neighborController = neighborTiles
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>()).ToList();

        for (var index = 0; index < neighborController.Count; index++)
        {
            var value = neighborController[index];

            if (mapController.LandformCheck(value) == false)
                continue;

            selectedTiles.Add(value);
            SelectBorder(value, ETileState.None);
        }

        if (tileController.gameObject.GetComponent<TileBase>().Structure?.IsAccessible == false
            || mapController.LandformCheck(tileController) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (neighborTiles.Contains(tileController.Model) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (tileController.gameObject.GetComponent<TileBase>().CurZombies != null)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (neighborTiles.Contains(tileController.Model))
        {
            SelectBorder(tileController, ETileState.Moveable);
        }
    }
    
    public void ExplorerPathFinder(TileController tileController, int num = 3)
    {
        int moveRange = 0;
        if (tileController.Model != mapController.Player.TileController.Model)
        {
            foreach (Coords coords in AStar.FindPath(mapController.Player.TileController.Model.Coords, tileController.Model.Coords))
            {
                if (moveRange == num)
                    break;

                var tile = TileToTileController(GetTileFromCoords(coords));

                if (mapController.LandformCheck(tile) == false)
                    continue;

                SelectBorder(tile, ETileState.None);
                selectedTiles.Add(tile);
                moveRange++;
            }

            if (moveRange != num && tileController.gameObject.GetComponent<TileBase>().Structure?.IsAccessible == false)
                SelectBorder(tileController, ETileState.Unable);
            else
                SelectBorder(tileController, ETileState.Moveable);
        }
        else
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }
    
    public void DisturbtorPathFinder(TileController tileController)
    {
        if (droneSelectedTiles.Contains(tileController))
        {
            var currentDistrubtor = mapController.DroneManager?.CurrentDistrubtor;
            if (currentDistrubtor != null)
            {
                currentDistrubtor.transform.position =
                    ((GameObject)tileController.Model.GameEntity).transform.position + Vector3.up;

                currentDistrubtor.GetComponent<Distrubtor>().DirectionObjectOff();

                if (mapController.LandformCheck(tileController))
                    SelectBorder(tileController, ETileState.Moveable);

                foreach (var item in mapController.Player.TileController.Model.Neighbours.Where(
                             item => item.Value == tileController.Model))
                {
                    currentDistrubtor.GetComponent<Distrubtor>().GetDirectionObject(item.Key).SetActive(true);
                }
            }
        }
        else
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }
    
    private void SelectBorder(TileController tileController, ETileState state)
    {
        switch (state)
        {
            case ETileState.None:
                tileController.GetComponent<Borders>().GetNormalBorder().SetActive(true);
                break;
            case ETileState.Moveable:
                tileController.GetComponent<Borders>().GetAvailableBorder().SetActive(true);
                break;
            case ETileState.Unable:
                tileController.GetComponent<Borders>().GetUnAvailableBorder().SetActive(true);
                break;
            case ETileState.Target:
                break;
        }

        selectedTiles.Add(tileController);
    }
    
    private void DeselectNormalBorder(TileController tileController)
    {
        tileController.GetComponent<Borders>().OffNormalBorder();

        if (selectedTiles.Contains(tileController))
            selectedTiles.Remove(tileController);
    }
    
    private void ClearTiles(List<TileController> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            TileController tile = tiles[i];
            DeselectNormalBorder(tile);
        }

        tiles.Clear();
    }
    
    public void InvalidateTilesCache()
    {
        _tilesCacheDirty = true;
    }
}
