using Events;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayingFieldBlocker : MonoBehaviour
{
	[SerializeField] private bool shouldBlockRaycasts;

	private void Start()
	{
		var button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		GameBus.OnBlockUI.Publish(shouldBlockRaycasts);
	}
}