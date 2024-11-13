using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CustomButton : Button, IPointerEnterHandler
{
    public UnityEvent onPointerEnter = new UnityEvent();

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        // onPointerEnter.Invoke();
    }
}