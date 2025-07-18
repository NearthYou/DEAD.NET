using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hexamap;

public class ZombieManager : MonoBehaviour
{
    [Header("좀비 설정")] [Space(5f)] [SerializeField]
    private float zombieSpawnHeight = 0.6f;
    
    [SerializeField] private Transform zombiesTransform;
    [SerializeField] private MapPrefabSO mapPrefab;
    
    private List<GameObject> zombiesList = new List<GameObject>();
    private MapController mapController;
    
    public List<GameObject> ZombiesList => zombiesList;
    
    public void Initialize(MapController controller, Transform zombieParent, MapPrefabSO prefab)
    {
        mapController = controller;
        zombiesTransform = zombieParent;
        mapPrefab = prefab;
    }
    
    public void SpawnZombies(int zombiesNumber, MapData mapData)
    {
        var tileList = mapController.GetAllTiles();
        var selectedTiles = mapController.RandomTileSelect(EObjectSpawnType.ExcludePlayer, zombiesNumber);

        for (int i = 0; i < selectedTiles.Count; i++)
        {
            var tile = tileList[selectedTiles[i]];
            var spawnPos = ((GameObject)tile.GameEntity).transform.position;
            spawnPos.y += zombieSpawnHeight;

            var zombie = Instantiate(mapPrefab.items[(int)EMabPrefab.Zombie].prefab, spawnPos,
                Quaternion.Euler(0, Random.Range(0, 360), 0), zombiesTransform);
            zombie.name = "Zombie " + (i + 1);
            zombie.GetComponent<ZombieBase>().Init(tile);
            zombie.GetComponent<ZombieBase>().SetValue(mapData.playerMovementPoint, mapData.zombieDetectionRange);
            zombiesList.Add(zombie);
        }
    }
    
    public void SpawnTutorialZombie()
    {
        var tile = mapController.GetTileFromCoords(new Coords(0, -2));
        var spawnPos = ((GameObject)tile.GameEntity).transform.position;
        spawnPos.y += zombieSpawnHeight;

        var zombie = Instantiate(mapPrefab.items[(int)EMabPrefab.Zombie].prefab, spawnPos,
            Quaternion.Euler(0, Random.Range(0, 360), 0), zombiesTransform);
        zombie.name = "Tutorial Zombie";
        zombie.GetComponent<ZombieBase>().Init(tile);
        zombiesList.Add(zombie);
    }
    
    public void SpawnStructureZombies(List<TileBase> tiles)
    {
        var randomInt = Random.Range(0, tiles.Count);
        var tile = tiles[randomInt];
        var spawnPos = tile.transform.position;
        spawnPos.y += zombieSpawnHeight;

        var zombie = Instantiate(mapPrefab.items[(int)EMabPrefab.Zombie].prefab, spawnPos,
            Quaternion.Euler(0, Random.Range(0, 360), 0), zombiesTransform);
        zombie.name = "Structure Zombie";
        zombie.GetComponent<ZombieBase>().Init(tile.GetComponent<TileController>().Model);
        zombie.GetComponent<ZombieBase>().Stun();
        zombiesList.Add(zombie);
    }
    
    public IEnumerator HandleZombies()
    {
        bool zombieActEnd = false;

        for (var index = 0; index < zombiesList.Count; index++)
        {
            var zombie = zombiesList[index];
            zombie.GetComponent<ZombieBase>().DetectionAndAct();

            if (index == zombiesList.Count - 1)
                zombieActEnd = true;
        }

        yield return new WaitUntil(() => zombieActEnd);
        yield return new WaitForSeconds(1f);
        CheckSumZombies();
    }
    
    public void CheckSumZombies()
    {
        List<ZombieBase> zombieBases = zombiesList.Select(x => x.GetComponent<ZombieBase>()).ToList();
        List<ZombieBase> removeZombies = new List<ZombieBase>();

        for (int i = 0; i < zombieBases.Count; i++)
        {
            for (int j = i + 1; j < zombieBases.Count; j++)
            {
                var firstZombies = zombieBases[i];
                var secondZombies = zombieBases[j];

                if (firstZombies.zombieData.count == 0 || secondZombies.zombieData.count == 0)
                    continue;

                if (firstZombies.curTile == secondZombies.curTile)
                {
                    firstZombies.SumZombies(secondZombies);
                    removeZombies.Add(secondZombies);
                }
            }
        }

        for (int i = 0; i < removeZombies.Count(); i++)
        {
            var item = removeZombies[i];
            zombiesList.Remove(item.gameObject);
            Destroy(item.gameObject);
        }
    }
    
    public bool CheckZombiesNearPlayer(Player player, int detectionRange)
    {
        var playerNearTiles = mapController.GetTilesInRange(player.TileController.Model, detectionRange);

        for (int i = 0; i < zombiesList.Count; i++)
        {
            GameObject item = zombiesList[i];
            if (playerNearTiles.Contains(item.GetComponent<ZombieBase>().curTile))
            {
                return true;
            }
        }
        return false;
    }
    
    public void ClearAllZombies()
    {
        foreach (var zombie in zombiesList)
        {
            if (zombie != null)
                Destroy(zombie.gameObject);
        }
        zombiesList.Clear();
    }
} 