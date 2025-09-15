using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUiController : ControllerBase
{
    [Header("UI Components")]
    [SerializeField] private GameObject disturbtorButton;
    [SerializeField] private GameObject explorerButton;
    [SerializeField] private TargetPointUI targetPoint;
    [SerializeField] private TileInfoPanel tileInfoPanel;

    private void Start()
    {
        App.instance.AddController(this);
    }

    public override EControllerType GetControllerType()
    {
        return EControllerType.MAP;
    }

    public void DisturbtorButtonInteractableOn()
    {
        disturbtorButton.GetComponentInChildren<Button>().interactable = true;
    }

    public bool GetActiveTileInfo()
    {
        return tileInfoPanel.gameObject.activeInHierarchy;
    }

    public void TrueTileInfo()
    {
        // var screenPoint = Camera.main.WorldToScreenPoint(tilePos);
        // tileInfoPanel.transform.position = screenPoint;
        if (UIManager.instance.isUIStatus("UI_MAP"))
            tileInfoPanel.gameObject.SetActive(true);
    }

    public void FalseTileInfo()
    {
        tileInfoPanel.gameObject.SetActive(false);
    }

    public void MoveTileInfo(Vector3 pos)
    {
        tileInfoPanel.transform.localPosition = pos;
    }

    public bool GetTileInfoActivate()
    {
        return tileInfoPanel.gameObject.activeInHierarchy;
    }

    public void OnPlayerMovePoint(Transform transform)
    {
        targetPoint.OnEffect(transform);
    }

    public void OffPlayerMovePoint()
    {
        targetPoint.OffEffect();
    }

    public void UpdateText(ETileInfoTMP infoTMP, string text)
    {
        tileInfoPanel.UpdateText(infoTMP, text);
    }

    public void UpdateImage(Sprite sprite)
    {
        tileInfoPanel.UpdateImage(sprite);
    }

    public bool MovePointActivate()
    {
        return targetPoint.ActivateStatus();
    }
    
    public void ExplorerButtonInteractable(bool isOn)
    {
        explorerButton.SetActive(isOn);
        explorerButton.GetComponentInChildren<Button>().interactable = isOn;
    }
    
    public void DistrubtorButtonInteractable(bool isOn)
    {
        disturbtorButton.SetActive(isOn);
        disturbtorButton.GetComponentInChildren<Button>().interactable = isOn;
    }
}
