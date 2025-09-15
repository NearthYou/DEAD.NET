using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class StructureBase
{
    protected string structureName;
    protected Resource resource;
    protected bool isUse;
    protected bool isAccessible;
    protected List<TileBase> neighborTiles;
    protected List<TileBase> colleagues;
    protected int visitDay;
    
    public ItemData specialItem;
    public GameObject structureModel;

    public string StructureName => structureName;
    public Resource Resource => resource;
    public bool IsUse => isUse;
    public bool IsAccessible => isAccessible;
    public List<TileBase> NeighborTiles => neighborTiles;
    public List<TileBase> Colleagues => colleagues;
    public int VisitDay => visitDay;
    
    public abstract void Init(List<TileBase> _neighborTiles, GameObject _structureModel, ItemSO _itemSO);
    public abstract void YesFunc();
    public abstract void NoFunc();
    
    public void SetIsUse(bool _isUse)
    {
        isUse = _isUse;
        visitDay = UIManager.instance.GetNoteController().dayCount;
    }
    
    public void SetColleagues(List<TileBase> _colleagues)
    {
        colleagues = _colleagues;
    }
    
    public void AllowAccess()
    {
        isUse = true;
        isAccessible = true;
    }
}