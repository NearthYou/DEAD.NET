using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    bool isMouseEnter = false;

    [SerializeField] Sprite city;
    [SerializeField] Sprite jungle;
    [SerializeField] Sprite desert;
    [SerializeField] Sprite tundra;

    Image image;
    Player player;
    [HideInInspector] ETileType tileType;

    void Start()
    {
        image = gameObject.GetComponent<Image>();

        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return new WaitUntil(() => MapController.Instance);
        yield return new WaitUntil(() => MapController.Instance.Player != null);

        player = MapController.Instance.Player;
        SetIconImage();
    }

    void Update()
    {
        if (isMouseEnter == true)
            ShowMapInfo();
    }

    public virtual void ShowMapInfo()
    {
        Vector3 mousePos = Input.mousePosition;
        UIManager.instance.GetInfoController().ShowMapInfo(tileType, mousePos);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isMouseEnter == false)
        {
            isMouseEnter = true;
            UIManager.instance.GetInfoController().isNew = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isMouseEnter == true)
        {
            isMouseEnter = false;
            UIManager.instance.GetInfoController().HideInfo();
        }
    }

    public void SetIconImage()
    {
        var tile = player.TileController;
        tileType = tile.GetComponent<TileBase>().TileType;

        switch (tileType)
        {
            case ETileType.None:
                image.sprite = city;
                break;
            case ETileType.Jungle:
                image.sprite = jungle;
                break;
            case ETileType.Desert:
                image.sprite = desert;
                break;
            case ETileType.Tundra:
                image.sprite = tundra;
                break;
        }
    }
}
