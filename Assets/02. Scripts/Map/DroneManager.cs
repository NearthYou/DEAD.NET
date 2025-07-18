using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hexamap;
using DG.Tweening;

public class DroneManager : MonoBehaviour
{
    [Header("드론 설정")] [Space(5f)] [SerializeField]
    private Transform mapTransform;
    [SerializeField] private MapPrefabSO mapPrefab;
    
    private List<GameObject> distrubtors = new List<GameObject>();
    private List<GameObject> explorers = new List<GameObject>();
    private GameObject curDistrubtor;
    private GameObject curExplorer;
    private MapController mapController;
    private Player player;
    
    public List<GameObject> Distrubtors => distrubtors;
    public List<GameObject> Explorers => explorers;
    public GameObject CurrentDistrubtor => curDistrubtor;
    public GameObject CurrentExplorer => curExplorer;
    
    public void Initialize(MapController controller, Player playerRef, Transform mapParent, MapPrefabSO prefab)
    {
        mapController = controller;
        player = playerRef;
        mapTransform = mapParent;
        mapPrefab = prefab;
    }
    
    public void PreparingDistrubtor(bool set)
    {
        if (set)
        {
            var neighborTiles = mapController.GetTilesInRange(player.TileController.Model, 1);
            var neighborController = neighborTiles
                .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>()).ToList();

            for (var index = 0; index < neighborController.Count; index++)
            {
                var value = neighborController[index];
                if (mapController.LandformCheck(value) == false)
                    continue;
                mapController.AddToDroneSelectedTiles(value);
                mapController.SelectTargetBorder(value);
            }

            GenerateExampleDisturbtor();
            App.instance.GetMapManager().SetIsDronePrepared(true, "Distrubtor");
        }
        else
        {
            distrubtors.Remove(curDistrubtor);
            App.instance.GetMapManager().SetIsDronePrepared(false, "Distrubtor");
            UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_DISTURBE");
            Destroy(curDistrubtor);
            mapController.DeselectAllTargetTiles();
        }
    }
    
    private void GenerateExampleDisturbtor()
    {
        curDistrubtor = Instantiate(mapPrefab.items[(int)EMabPrefab.Disturbtor].prefab,
            player.transform.position + Vector3.up * 1.5f, Quaternion.Euler(0, 90, 0));
        curDistrubtor.transform.parent = mapTransform;
        curDistrubtor.GetComponentInChildren<MeshRenderer>(true).material.DOFade(50, 0);
        distrubtors.Add(curDistrubtor);
    }
    
    public void InstallDistrubtor(TileController tileController, CompassPoint direction)
    {
        curDistrubtor.GetComponent<Distrubtor>().Set(tileController.Model, direction);
        curDistrubtor.GetComponent<Distrubtor>().DirectionObjectOff();

        var droneSelectedTiles = mapController.GetDroneSelectedTiles();
        for (int i = 0; i < droneSelectedTiles.Count; i++)
        {
            mapController.DeselecTargetBorder(droneSelectedTiles[i]);
        }

        App.instance.GetMapManager().SetIsDronePrepared(false, "Distrubtor");
    }
    
    public void PreparingExplorer(bool set)
    {
        if (set)
        {
            GenerateExampleExplorer();
            App.instance.GetMapManager().SetIsDronePrepared(true, "Explorer");
        }
        else
        {
            explorers.Remove(curExplorer);
            App.instance.GetMapManager().SetIsDronePrepared(false, "Explorer");
            UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_FINDOR");
            Destroy(curExplorer);
        }
    }
    
    private void GenerateExampleExplorer()
    {
        curExplorer = Instantiate(mapPrefab.items[(int)EMabPrefab.Explorer].prefab,
            player.transform.position + Vector3.up * 1.5f, Quaternion.Euler(0, 90, 0));
        curExplorer.transform.parent = mapTransform;
        curExplorer.GetComponentInChildren<MeshRenderer>().material.DOFade(50, 0);
        curExplorer.GetComponent<Explorer>().Set(player.TileController.Model);
        explorers.Add(curExplorer);
    }
    
    public void InstallExplorer(TileController tileController)
    {
        curExplorer.GetComponent<Explorer>().Targeting(tileController.Model);
        curExplorer.GetComponent<Explorer>().Move();
        App.instance.GetMapManager().SetIsDronePrepared(false, "");
    }
    
    public IEnumerator HandleDrones()
    {
        // 교란기 처리
        if (distrubtors.Count > 0 && distrubtors != null)
        {
            for (int i = 0; i < distrubtors.Count; i++)
            {
                distrubtors[i].GetComponent<Distrubtor>().Move();
            }
        }

        // 탐사기 처리
        if (explorers.Count > 0 && explorers != null)
        {
            for (int i = 0; i < explorers.Count; i++)
            {
                StartCoroutine(explorers[i].GetComponent<Explorer>().Move());
            }
        }
        
        yield return null;
    }
    
    public void RemoveDistrubtor(Distrubtor distrubtor)
    {
        distrubtors.Remove(distrubtor.gameObject);
    }
    
    public void RemoveExplorer(Explorer explorer)
    {
        explorers.Remove(explorer.gameObject);
    }
    
    public void InvocationExplorers()
    {
        for (int i = 0; i < explorers.Count; i++)
        {
            var item = explorers[i].GetComponent<Explorer>();
            if (item.GetIsIdle())
                item.GetComponent<Explorer>().Invocation();
        }
    }
    
    public Distrubtor CalculateDistanceToDistrubtor(Tile tile, int range)
    {
        var searchTiles = mapController.GetTilesInRange(tile, range);

        if (distrubtors.Count <= 0)
            return null;

        for (var i = 0; i < searchTiles.Count; i++)
        {
            var item = searchTiles[i];

            for (var index = 0; index < distrubtors.Count; index++)
            {
                var distrubtor = distrubtors[index];

                if (distrubtor.GetComponent<Distrubtor>().currentTile == item)
                    return distrubtor.GetComponent<Distrubtor>();
            }
        }

        return null;
    }
} 