using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] int collectiveAbility;
    [SerializeField] ItemSO itemSO;

    private List<Resource> owendResources;
    private List<Resource> lastResources;
    bool isGetResource;

    public bool IsGetResource
    {
        get { return isGetResource; }
        set { isGetResource = value; }
    }

    void Start()
    {
        owendResources = new List<Resource>();
        lastResources = new List<Resource>();
    }

    public void GetResource(TileController tile)
    {
        int collectiveCount = collectiveAbility;

        if (App.instance.GetMapManager().IsJungleTile(tile))
            collectiveCount += 1;

        lastResources = tile.GetComponent<TileBase>().GetResources(collectiveAbility);

        for (int i = 0; i < lastResources.Count; i++)
        {
            if (owendResources.Exists(x => x.ItemBase == lastResources[i].ItemBase))
            {
                var resource = owendResources.Find(x => x.ItemBase == lastResources[i].ItemBase);

                if (resource.ItemCount <= 0)
                    return;
                else
                    resource.ItemCount += lastResources[i].ItemCount;

                var item = itemSO.items.ToList().Find(x => x == resource.ItemBase);

                //Debug.LogFormat("{0} 자원 {1}개 획득", item.data.Korean, lastResources[i].ItemCount);
                isGetResource = true;
            }
            else
            {
                owendResources.Add(lastResources[i]);
                var item = itemSO.items.ToList().Find(x => x == lastResources[i].ItemBase);

                //Debug.LogFormat("새로운 자원 {0} {1}개 획득했다.", item.data.Korean, lastResources[i].ItemCount);
                isGetResource = true;
            }
        }

        for (int i = 0; i < lastResources.Count; i++)
        {
            ItemBase item = itemSO.items.ToList().Find(x => x == lastResources[i].ItemBase);

            for (int j = 0; j < lastResources[i].ItemCount; j++)
            {
                //TODO :: SFX 재생, 아이템 획득 스크립트 추첨
                UIManager.instance.GetInventoryController().AddItem(item);
                PlaySFX(item.sfxName);
            }
        }
    }

    public void PlaySFX(string str)
    {
        App.instance.GetSoundManager().PlaySFX(str);
    }

    public bool CheckResource(TileController tileController)
    {
        return tileController.GetComponent<TileBase>().CheckResources();
    }

    public List<Resource> GetLastResources()
    {
        if (lastResources != null || lastResources.Count > 0)
        {
            return lastResources;
        }
        else
        {
            return null;
        }
    }
}