using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongClickButton : Selectable, IPointerDownHandler, IPointerUpHandler
{
	private bool pointerDown;
	private float pointerDownTimer;

	[SerializeField]
	private float requiredHoldTime = 1;

	public UnityEvent onLongClick;

	[SerializeField]
	private Image fillImage = null;

	public override void OnPointerDown(PointerEventData eventData)
	{
        if (!IsActive() || !IsInteractable())
            return;
		pointerDown = true;
		// Debug.Log("OnPointerDown");
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
        if (!IsActive() || !IsInteractable())
            return;
        pointerDown = false;
        pointerDownTimer = 0;
        fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
		Reset();
		// Debug.Log("OnPointerUp");
	}

	private void Update()
	{
		if (pointerDown)
		{
			pointerDownTimer += Time.deltaTime;
			if (pointerDownTimer >= requiredHoldTime)
			{
				if (onLongClick != null)
					onLongClick.Invoke();

				pointerDown = false;
                pointerDownTimer = 0;
                fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
                Reset();
			}
			fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
		}
	}

	// private void Reset()
	// {
	// 	pointerDown = false;
	// 	pointerDownTimer = 0;
	// 	fillImage.fillAmount = pointerDownTimer / requiredHoldTime;
	// }

}