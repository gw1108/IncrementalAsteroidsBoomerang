using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string tooltipHeader;
    public string tooltipMessage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TryShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnMouseEnter()
    {
        TryShowTooltip();
    }

    private void OnMouseExit()
    {
        HideTooltip();
    }

    private void TryShowTooltip()
    {
        if (!string.IsNullOrWhiteSpace(tooltipMessage) || !string.IsNullOrWhiteSpace(tooltipHeader))
        {
            TooltipManager.Instance.SetAndShowTooltip(tooltipMessage, tooltipHeader, transform);
        }
    }

    private void HideTooltip()
    {
        TooltipManager.Instance.HideTooltip();
    }
}
