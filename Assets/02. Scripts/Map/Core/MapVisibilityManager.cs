using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Hexamap;

public class MapVisibilityManager : MonoBehaviour
{
    private List<Tile> sightTiles = new List<Tile>();
    private HashSet<Tile> _cachedPlayerSightTiles;
    private bool _playerSightCacheDirty = true;
    
    private MapController mapController;
    private MapTileManager tileManager;
    private MapRenderingManager renderingManager;
    private MapSettings mapSettings;
    
    public void Initialize(MapController mapController, MapTileManager tileManager, MapRenderingManager renderingManager, MapSettings mapSettings)
    {
        this.mapController = mapController;
        this.tileManager = tileManager;
        this.renderingManager = renderingManager;
        this.mapSettings = mapSettings;
    }
    
    public void OcclusionCheck(Tile _targetTile)
    {
        sightTiles = tileManager.GetTilesInRange(_targetTile, mapSettings.sightRange);
        sightTiles.Add(_targetTile);

        _playerSightCacheDirty = true;
        
        OptimizeRenderingAsync().Forget();
        
        UpdateAllTileVisibilityAsync().Forget();
    }
    
    private async UniTask OptimizeRenderingAsync()
    {
        await renderingManager.OptimizeRenderingAsync(sightTiles);
    }
    
    private async UniTask UpdateAllTileVisibilityAsync()
    {
        if (mapController.Player?.TileController?.Model == null) return;
        
        var playerSightTiles = GetPlayerSightTiles();
        var sightHashSet = new HashSet<Tile>(playerSightTiles);
        
        var allTileControllers = tileManager.GetAllTiles()
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>())
            .Where(x => x != null)
            .ToList();
        
        var batchSize = 50;
        
        for (int i = 0; i < allTileControllers.Count; i += batchSize)
        {
            int endIndex = Mathf.Min(i + batchSize, allTileControllers.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                var tileController = allTileControllers[j];
                if (tileController != null)
                {
                    var tileBase = tileController.GetComponent<TileBase>();
                    if (tileBase != null)
                    {
                        bool inSight = sightHashSet.Contains(tileController.Model);
                        tileBase.UpdateVisibilityFromController(inSight);
                    }
                }
            }
            
            if (i + batchSize < allTileControllers.Count)
                await UniTask.Yield();
        }
    }
    
    public List<Tile> GetPlayerSightTiles()
    {
        if (_playerSightCacheDirty || _cachedPlayerSightTiles == null)
        {
            if (_cachedPlayerSightTiles == null)
                _cachedPlayerSightTiles = new HashSet<Tile>();
            else
                _cachedPlayerSightTiles.Clear();
                
            var tiles = tileManager.GetTilesInRange(mapController.Player.TileController.Model, mapSettings.playerSightRange);
            foreach (var tile in tiles)
            {
                _cachedPlayerSightTiles.Add(tile);
            }
            _playerSightCacheDirty = false;
        }
        
        return _cachedPlayerSightTiles.ToList();
    }
    
    public List<Tile> GetPlayerSightTilesForParticles()
    {
        var list = tileManager.GetTilesInRange(mapController.Player.TileController.Model, mapSettings.playerSightRange + 3);
        return list;
    }

    public List<Tile> GetSightTiles(Tile tile)
    {
        var list = tileManager.GetTilesInRange(tile, mapSettings.playerSightRange);
        return list;
    }
    
    public void InvalidatePlayerSightCache()
    {
        _playerSightCacheDirty = true;
    }
}
