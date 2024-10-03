using Events;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindowView : MonoBehaviour
{
	[SerializeField] private GraphicRaycaster activeCanvas = default;

	private void Start()
	{
		GameBus.OnBlockUI.Subscribe(this, DisableRaycaster);
	}

	private void DisableRaycaster(bool value)
	{
		activeCanvas.enabled = !value;
	}
}