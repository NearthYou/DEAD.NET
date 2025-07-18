public enum ESceneType
{
    Title,
    Game,
    UI,
    Map,
    Crafting
}

public enum EUILayer
{
    Base,
    UI,
    Setting
}

public enum EItemType
{
    Material,
    Equipment,
    Special
}

public enum ENotePageType
{
    Result,
    Select
}

public enum ESlotType
{
    InventorySlot,
    CraftingSlot,
    ResultSlot,
    EquipSlot,
    BlueprintSlot,
}

public enum EQuestType
{
    Tutorial,
    Main,
    Sub
}

public enum EAlertType
{
    Selection,
    Caution
}

public enum ECraftModeType
{
    Craft,
    Equip,
    Blueprint
}

public enum EScriptType
{
    Diary,
    Resource,
    MainQuest,
    SubQuest
}

public enum ESoundType
{
    SFX,
    BGM,
    ALL
}

#region Character

public enum EHungerType
{
    Normal,
    Hunger,
    Starving
};
public enum EThirstType
{
    Normal,
    Thirst,
    Dehydration
};
public enum EConditionType
{
    Normal,
    Anx,
    Crazy,
    Machine
};
public enum EBodyHealthType
{
    Normal,
    Hurt,
    Injury,
    Disease
};
public enum EInfectionType
{
    Normal,
    Bite,
    Zombie
};
public enum EPartsType
{
    None,
    Eyeball,
    LeftArm,
    RightArm,
    Body,
    Legs
};

#endregion

#region HexTile

public enum ETileType
{
    None,
    Jungle,
    Desert,
    Tundra,
    Neo
}

public enum EResourceType
{
    Steel,
    Carbon,
    Plasma,
    Powder,
    Gas,
    Rubber,
    Wire
}

public enum ETileState
{
    None,
    Moveable,
    Unable,
    Target
}

public enum EMabPrefab
{
    Player,
    Zombie,
    Disturbtor,
    Explorer,
    Tower,
    Production,
    Army
}

public enum EStructure
{
    Tower,
    Production,
    Army,
    Dynamo
}

public enum EObjectSpawnType
{
    ExcludePlayer,
    IncludePlayer,
    ExcludeEntites,
    IncludeEntites
}

public enum ETileMouseState
{
    Nothing,
    CanClick,
    CanPlayerMove,
    DronePrepared
}

public enum StructureType
{
    Tower,
    Production,
    Army
}

#endregion