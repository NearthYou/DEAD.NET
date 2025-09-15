public class Resource
{
    private string itemCode;
    private int itemCount;
    private ItemBase itemBase;

    public string ItemCode
    {
        get => itemCode;
        set => itemCode = value;
    }

    public int ItemCount
    {
        get => itemCount;
        set
        {
            if (value < 0)
                itemCount = 0;
            else
                itemCount = value;
        }
    }

    public ItemBase ItemBase
    {
        get => itemBase;
        set => itemBase = value;
    }

    public Resource(string _itemCode, int _itemCount)
    {
        this.ItemCode = _itemCode;
        this.ItemCount = _itemCount;
    }
    public Resource(string _itemCode, int _itemCount, ItemBase _itemBase)
    {
        this.ItemCode = _itemCode;
        this.ItemCount = _itemCount;
        this.ItemBase = _itemBase;
    }
    
    public void InitItemBase(ItemBase _itemBase)
    {
        this.ItemBase = _itemBase;
    }
}