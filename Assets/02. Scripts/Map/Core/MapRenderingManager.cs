using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using Hexamap;

public class MapRenderingManager : MonoBehaviour
{
    private Dictionary<GameObject, Renderer> _rendererCache = new Dictionary<GameObject, Renderer>();
    private Dictionary<GameObject, bool> _lastVisibilityState = new Dictionary<GameObject, bool>();
    private HashSet<GameObject> _visibleObjects = new HashSet<GameObject>();
    private HashSet<GameObject> _invisibleObjects = new HashSet<GameObject>();
    
    private List<StructureObject> _cachedStructureObjects;
    private List<TileController> _cachedTileControllers;
    private HashSet<Tile> _cachedVisibleTiles;
    
    private List<GameObject> _tempShowList;
    private List<GameObject> _tempHideList;
    
    private Transform objectsTransform;
    private MapController mapController;
    
    public void Initialize(Transform objectsTransform, MapController mapController)
    {
        this.objectsTransform = objectsTransform;
        this.mapController = mapController;
    }
    
    public async UniTask OptimizeRenderingAsync(List<Tile> sightTiles)
    {
        OptimizeStructureRendering(sightTiles);
        await UniTask.Yield();
        
        OptimizeTileRendering(sightTiles);
        await UniTask.Yield();
        
        await UpdateParticleLODAsync();
        await UniTask.Yield();
        
        CleanupVisibilityCache();
    }
    
    private void OptimizeStructureRendering(List<Tile> sightTiles)
    {
        if (_cachedStructureObjects == null)
        {
            _cachedStructureObjects = objectsTransform.GetComponentsInChildren<StructureObject>(true).ToList();
        }
        
        List<StructureObject> structureObjects = _cachedStructureObjects;

        for (int i = 0; i < structureObjects.Count; i++)
        {
            StructureObject item = structureObjects[i];
            GameObject obj = item.gameObject;
            
            bool shouldBeVisible = sightTiles.Contains(item.CurTile);
            
            if (_lastVisibilityState.TryGetValue(obj, out bool lastState))
            {
                if (lastState != shouldBeVisible)
                {
                    SetObjectVisibility(obj, shouldBeVisible);
                    _lastVisibilityState[obj] = shouldBeVisible;
                }
            }
            else
            {
                SetObjectVisibility(obj, shouldBeVisible);
                _lastVisibilityState[obj] = shouldBeVisible;
            }
        }
    }

    private void OptimizeTileRendering(List<Tile> sightTiles)
    {
        if (_cachedTileControllers == null)
        {
            var allTiles = mapController.GetAllTiles();
            _cachedTileControllers = allTiles
                .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>())
                .Where(x => x != null)
                .ToList();
        }
        
        if (_cachedVisibleTiles == null)
        {
            _cachedVisibleTiles = new HashSet<Tile>();
        }
        else
        {
            _cachedVisibleTiles.Clear();
        }
        
        foreach (var tile in sightTiles)
        {
            _cachedVisibleTiles.Add(tile);
        }
        
        if (_tempShowList == null)
            _tempShowList = new List<GameObject>();
        else
            _tempShowList.Clear();
            
        if (_tempHideList == null)
            _tempHideList = new List<GameObject>();
        else
            _tempHideList.Clear();
        
        for (int i = 0; i < _cachedTileControllers.Count; i++)
        {
            TileController tileController = _cachedTileControllers[i];
            if (tileController == null) continue;
            
            GameObject tileObj = tileController.gameObject;
            bool shouldBeVisible = _cachedVisibleTiles.Contains(tileController.Model);
            
            if (_lastVisibilityState.TryGetValue(tileObj, out bool lastState))
            {
                if (lastState != shouldBeVisible)
                {
                    if (shouldBeVisible)
                        _tempShowList.Add(tileObj);
                    else
                        _tempHideList.Add(tileObj);
                    
                    _lastVisibilityState[tileObj] = shouldBeVisible;
                }
            }
            else
            {
                if (shouldBeVisible)
                    _tempShowList.Add(tileObj);
                else
                    _tempHideList.Add(tileObj);
                
                _lastVisibilityState[tileObj] = shouldBeVisible;
            }
        }
        
        BatchSetVisibility(_tempShowList, true);
        BatchSetVisibility(_tempHideList, false);
    }
    
    private void SetObjectVisibility(GameObject obj, bool visible)
    {
        if (obj == null) return;
        
        if (!_rendererCache.TryGetValue(obj, out Renderer renderer))
        {
            renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                _rendererCache[obj] = renderer;
        }
        
        if (renderer != null)
        {
            renderer.enabled = visible;
            
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        else
        {
            obj.SetActive(visible);
        }
        
        if (visible)
            _visibleObjects.Add(obj);
        else
            _invisibleObjects.Add(obj);
    }

    private void BatchSetVisibility(List<GameObject> objects, bool visible)
    {
        if (objects.Count == 0) return;
        
        var rendererGroups = new Dictionary<Renderer, List<GameObject>>();
        
        foreach (var obj in objects)
        {
            if (!_rendererCache.TryGetValue(obj, out Renderer renderer))
            {
                renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                    _rendererCache[obj] = renderer;
            }
            
            if (renderer != null)
            {
                if (!rendererGroups.ContainsKey(renderer))
                    rendererGroups[renderer] = new List<GameObject>();
                rendererGroups[renderer].Add(obj);
            }
        }
        
        foreach (var group in rendererGroups)
        {
            Renderer renderer = group.Key;
            renderer.enabled = visible;
            
            Renderer[] childRenderers = renderer.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        
        foreach (var obj in objects)
        {
            if (!_rendererCache.ContainsKey(obj))
            {
                obj.SetActive(visible);
            }
        }
    }

    private void CleanupVisibilityCache()
    {
        var keysToRemove = new List<GameObject>();
        
        foreach (var kvp in _lastVisibilityState)
        {
            if (kvp.Key == null)
                keysToRemove.Add(kvp.Key);
        }
        
        foreach (var key in keysToRemove)
        {
            _lastVisibilityState.Remove(key);
            _rendererCache.Remove(key);
            _visibleObjects.Remove(key);
            _invisibleObjects.Remove(key);
        }
        
        if (_lastVisibilityState.Count > 1000)
        {
            var oldestKeys = _lastVisibilityState.Keys.Take(100).ToList();
            foreach (var key in oldestKeys)
            {
                _lastVisibilityState.Remove(key);
                _rendererCache.Remove(key);
            }
        }
    }
    
    private async UniTask UpdateParticleLODAsync()
    {
        var mapManager = App.instance.GetMapManager();
        if (mapManager != null)
        {
            await mapManager.UpdateAllParticleLODAsync();
        }
    }
    
    public void ClearRenderingCache()
    {
        _rendererCache.Clear();
        _lastVisibilityState.Clear();
        _visibleObjects.Clear();
        _invisibleObjects.Clear();
        _cachedStructureObjects = null;
        _cachedTileControllers = null;
        _cachedVisibleTiles = null;
    }
    
    public void InvalidateStructureCache()
    {
        _cachedStructureObjects = null;
    }
}
