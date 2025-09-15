using System;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using System.Linq;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

[SelectionBase]
public class TileBase : MonoBehaviour
{
    [Header("Tile Data")]
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private ETileType tileType;
    [SerializeField] private Sprite landformSprite;
    [SerializeField] private SpriteRenderer[] resourceIcons;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem[] tileParticles;
    [SerializeField] private bool hasEnvironmentalEffect = false;

    private Dictionary<EResourceType, int> gachaProbability;
    private List<EResourceType> gachaList;
    private List<Resource> appearanceResources;

    private Tile tile;
    private TileData tileData;
    private bool inPlayerSight;
    private bool isStructureNeighbor;
    private string resourceText;
    private string landformText;
    private ZombieBase curZombies;
    private StructureBase structure;
    private GameObject structureObject;

    public Tile Tile => tile;
    public TileData TileData => tileData;
    public bool IsStructureNeighbor => isStructureNeighbor;
    public ZombieBase CurZombies => curZombies;
    public StructureBase Structure => structure;
    public GameObject StructureObject => structureObject;
    public ETileType TileType => tileType;
    public bool HasEnvironmentalEffect => hasEnvironmentalEffect;

    private void Awake()
    {
        gachaProbability = new Dictionary<EResourceType, int>();
        gachaList = new List<EResourceType>();
        appearanceResources = new List<Resource>();

        App.instance.GetDataManager().tileData.TryGetValue(GetTileDataIndex(), out TileData data);
        tileData = data;
        landformText = tileData.Korean;
    }

    private void Start()
    {
        Player.PlayerSightUpdate += CheckPlayerTIle;
        tile = GetComponent<TileController>().Model;
        
        InitializeParticleEffects();
    }

    private void OnDestroy()
    {
        Player.PlayerSightUpdate -= CheckPlayerTIle;
    }

    private void GetTilData()
    {
        gachaList.Clear();
        appearanceResources.Clear();
        gachaProbability.Clear();

        gachaProbability.Add(EResourceType.Steel, tileData.RemainPossibility_Steel);
        gachaProbability.Add(EResourceType.Carbon, tileData.RemainPossibility_Carbon);
        gachaProbability.Add(EResourceType.Plasma, tileData.RemainPossibility_Plasma);
        gachaProbability.Add(EResourceType.Powder, tileData.RemainPossibility_Powder);
        gachaProbability.Add(EResourceType.Gas, tileData.RemainPossibility_Gas);
        gachaProbability.Add(EResourceType.Rubber, tileData.RemainPossibility_Rubber);
    }

    public void SpawnRandomResource()
    {
        GetTilData();

        var randomInt = Random.Range(1, 3);

        while (gachaList.Count != randomInt)
        {
            var take = WeightedRandomizer.From(gachaProbability).TakeOne();
            if (gachaList.Contains(take) == false)
                gachaList.Add(take);
        }

        for (int i = 0; i < gachaList.Count; i++)
        {
            var itemBase = itemSO.items.ToList()
                .Find(x => x.data.English == gachaList[i].ToString());

            var resource = new Resource(gachaList[i].ToString(), Random.Range(1, 16), itemBase);
            appearanceResources.Add(resource);
        }

        RotationCheck(transform.rotation.eulerAngles);
    }

    public void ResourceUpdate(bool _isInPlayerSight)
    {
        if (structure != null)
        {
            if (structure.IsAccessible == false)
                return;
        }

        if (_isInPlayerSight)
        {
            ResourceInit();

            if (appearanceResources.Count > 0)
            {
                var text = "";

                for (int i = 0; i < appearanceResources.Count; i++)
                {
                    text += appearanceResources[i].ItemBase.data.Korean + " " +
                            appearanceResources[i].ItemCount + "EA\n";
                }

                resourceText = text;

                if (appearanceResources.Count == 1)
                {
                    resourceIcons[0].sprite = appearanceResources[0].ItemBase.itemImage;
                    resourceIcons[0].gameObject.SetActive(true);
                }
                else if (appearanceResources.Count == 2)
                {
                    for (int i = 0; i < appearanceResources.Count; i++)
                    {
                        SpriteRenderer item = resourceIcons[i + 1];
                        item.sprite = appearanceResources[i].ItemBase.itemImage;
                        item.gameObject.SetActive(true);
                    }
                }
                else
                {
                    for (int i = 0; i < appearanceResources.Count; i++)
                    {
                        SpriteRenderer item = resourceIcons[i + 3];
                        item.sprite = appearanceResources[i].ItemBase.itemImage;
                        item.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                resourceText = "자원 없음";

                for (int i = 0; i < resourceIcons.Length; i++)
                {
                    SpriteRenderer item = resourceIcons[i];
                    item.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            resourceText = "자원 : ???";

            for (int i = 0; i < resourceIcons.Length; i++)
            {
                SpriteRenderer item = resourceIcons[i];
                item.gameObject.SetActive(false);
            }
        }
    }

    void ResourceInit()
    {
        for (int i = 0; i < appearanceResources.Count; i++)
        {
            Resource item = appearanceResources[i];

            if (item.ItemCount == 0)
            {
                appearanceResources.Remove(item);
            }
        }

        for (int i = 0; i < resourceIcons.Length; i++)
        {
            SpriteRenderer icon = resourceIcons[i];
            icon.sprite = null;
            icon.gameObject.SetActive(false);
        }
    }


    protected void RotationCheck(Vector3 rotationValue)
    {
        if (appearanceResources.Count == 1)
        {
            SpriteRenderer item = resourceIcons[0];
            item.gameObject.transform.Rotate(0, 0, rotationValue.y + 90);
        }
        else if (appearanceResources.Count == 2)
        {
            if (Mathf.Abs(rotationValue.y) == 0 || Mathf.Abs(rotationValue.y) == 180)
            {
                for (int i = 0; i < resourceIcons.Length; i++)
                {
                    SpriteRenderer item = resourceIcons[i];
                    item.gameObject.transform.Rotate(0, 0, rotationValue.y + 90);
                }
            }
            else
            {
                resourceIcons[1].transform.parent.transform.localEulerAngles =
                    new Vector3(90, -rotationValue.y, 0);

                for (int i = 0; i < resourceIcons.Length; i++)
                {
                    SpriteRenderer item = resourceIcons[i];
                    item.gameObject.transform.Rotate(0, 0, 90);
                }
            }
        }
    }

    public List<Resource> GetResources(int count)
    {
        List<Resource> list = new List<Resource>();

        if (appearanceResources == null)
        {
            return null;
        }

        for (int i = 0; i < appearanceResources.Count; i++)
        {
            Resource item = appearanceResources[i];

            var itemBase = item.ItemBase;

            if (item.ItemCount - count >= 0)
            {
                list.Add(new Resource(item.ItemCode, count, itemBase));
                item.ItemCount -= count;
            }
            else
            {
                list.Add(new Resource(item.ItemCode, item.ItemCount, itemBase));
                item.ItemCount = 0;
            }
        }

        ResourceUpdate(true);
        return list;
    }

    public bool CheckResources()
    {
        if (appearanceResources != null)
            return true;
        else
            return false;
    }

    void CheckPlayerTIle()
    {
        if (App.instance.GetMapManager().mapController.GetPlayerSightTiles().Contains(tile))
        {
            ResourceUpdate(true);
        }
        else
        {
            ResourceUpdate(false);
        }
    }

    public void TileInfoUpdate()
    {
        App.instance.GetMapManager().mapUIController.UpdateImage(landformSprite);
        App.instance.GetMapManager().mapUIController.UpdateText(ETileInfoTMP.Landform, landformText);
        App.instance.GetMapManager().mapUIController.UpdateText(ETileInfoTMP.Resource, resourceText);

        if (curZombies == null)
            App.instance.GetMapManager().mapUIController.UpdateText(ETileInfoTMP.Zombie, "좀비 수 : ???");
        else
        {
            App.instance.GetMapManager().mapUIController
                .UpdateText(ETileInfoTMP.Zombie, "좀비 수 : " + curZombies.zombieData.count + "마리");
        }
    }

    public void SpawnQuestStructure(List<Tile> neighborTiles, GameObject _structureObject)
    {
        var neighborBases = neighborTiles
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileBase>()).ToList();

        structure = new Tower();
        ((Tower)structure).Init(neighborBases, _structureObject, itemSO);

        landformText = "타워";
        resourceText = "자원 : ???";

        for (int i = 0; i < resourceIcons.Length; i++)
        {
            SpriteRenderer item = resourceIcons[i];
            item.sprite = null;
            item.gameObject.SetActive(false);
        }

        TileInfoUpdate();
    }

    public void SpawnNormalStructure(StructureInfo structureInfo)
    {
        structureObject = structureInfo.StructureObject;

        StructureDataInput(structureInfo);
        
        for (int i = 0; i < resourceIcons.Length; i++)
        {
            SpriteRenderer item = resourceIcons[i];
            item.sprite = null;
            item.gameObject.SetActive(false);
        }
    }

    public void StructureDataInput(StructureInfo structureInfo)
    {
        var neighborBases = structureInfo.NeighborTiles
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileBase>()).ToList();

        var colleagueBases = structureInfo.ColleagueTiles
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileBase>()).ToList();

        
        switch (structureInfo.StructureType)
        {
            case EStructure.Production:
                structure = new ProductionStructure();
                ((ProductionStructure)structure).Init(neighborBases, structureInfo.StructureObject, itemSO);
                ((ProductionStructure)structure).SetColleagues(colleagueBases);

                landformText = "생산 공장";
                resourceText = "자원 : ???";
                break;
            case EStructure.Army:
                structure = new ArmyStructure();
                ((ArmyStructure)structure).Init(neighborBases, structureInfo.StructureObject, itemSO);
                ((ArmyStructure)structure).SetColleagues(colleagueBases);

                landformText = "군사 기지";
                resourceText = "자원 : ???";
                break;
                
        }
    }
    

    public void AddSpecialItem()
    {
        ItemBase itemBase;

        if (appearanceResources.Count == 2)
            appearanceResources.RemoveRange(appearanceResources.Count - 1, 1);

        if (structure.specialItem != null)
        {
            itemBase = itemSO.items.ToList()
                .Find(x => x.data == structure.specialItem);
        }
        else
        {
            itemBase = itemSO.items.ToList()
                .Find(x => x.data == structure.Resource.ItemBase.data);
        }

        // 특수 자원 추가
        appearanceResources.Add(new Resource(itemBase.English, 1, itemBase));

        RotationCheck(transform.rotation.eulerAngles);
        ResourceUpdate(true);
    }

    public void SetNeighborStructure()
    {
        isStructureNeighbor = true;
    }

    public void SetPlayerSight(bool inSight)
    {
        inPlayerSight = inSight;
        
        // 자원 아이콘 표시/숨김
        if (resourceIcons != null)
        {
            foreach (var icon in resourceIcons)
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(inSight);
                }
            }
        }
        
        // 구조물 표시/숨김
        if (structureObject != null)
        {
            structureObject.SetActive(inSight);
        }
    }

    private int GetTileDataIndex()
    {
        switch (tileType)
        {
            case ETileType.None:
                return 1001;
            case ETileType.Desert:
                return 1002;
            case ETileType.Tundra:
                return 1003;
            case ETileType.Jungle:
                return 1004;
            case ETileType.Neo:
                return 1005;
        }

        return 0;
    }

    /// <summary>
    /// 파티클 이펙트를 초기화합니다.
    /// </summary>
    private void InitializeParticleEffects()
    {
        SetupTileTypeParticles();
        
        var mapManager = App.instance.GetMapManager();
        if (mapManager != null)
        {
            mapManager.UpdateTileParticles(gameObject);
        }
    }
    
    /// <summary>
    /// 타일 타입에 따른 파티클을 설정합니다.
    /// </summary>
    private void SetupTileTypeParticles()
    {
        switch (tileType)
        {
            case ETileType.Desert:
                SetupDesertParticles();
                break;
            case ETileType.Tundra:
                SetupTundraParticles();
                break;
            case ETileType.Jungle:
                SetupJungleParticles();
                break;
            case ETileType.None:
                SetupNoneTileParticles();
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// 사막 타일의 파티클을 설정합니다.
    /// </summary>
    private void SetupDesertParticles()
    {
        hasEnvironmentalEffect = true;
    }
    
    /// <summary>
    /// 툰드라 타일의 파티클을 설정합니다.
    /// </summary>
    private void SetupTundraParticles()
    {
        hasEnvironmentalEffect = true;
    }
    
    /// <summary>
    /// 정글 타일의 파티클을 설정합니다.
    /// </summary>
    private void SetupJungleParticles()
    {
        hasEnvironmentalEffect = true;
    }
    
    /// <summary>
    /// 일반 타일의 파티클을 설정합니다.
    /// </summary>
    private void SetupNoneTileParticles()
    {
        hasEnvironmentalEffect = false;
    }
    
    /// <summary>
    /// 파티클 이펙트를 활성화/비활성화합니다.
    /// </summary>
    public void SetParticleEffectActive(bool active)
    {
        if (tileParticles != null)
        {
            foreach (var particle in tileParticles)
            {
                if (particle != null)
                {
                    if (active)
                        particle.Play();
                    else
                        particle.Stop();
                }
            }
        }
    }

    public void UpdateZombieInfo(ZombieBase zombie)
    {
        if (zombie == null)
        {
            curZombies = null;
            App.instance.GetMapManager().mapUIController.UpdateText(ETileInfoTMP.Zombie, "좀비 수 : ???");
        }
        else
        {
            curZombies = zombie;
            App.instance.GetMapManager().mapUIController
                .UpdateText(ETileInfoTMP.Zombie, "좀비 수 : " + curZombies.zombieData.count + "마리");
        }
    }

    public virtual void TileEffectInit(Player player, ZombieBase zombie)
    {
    }
}