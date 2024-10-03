using Global;
using UnityEngine;
using TMPro;

namespace Lobby
{
    public partial class OnlineCounterView : View
    {
        [SerializeField] private TextMeshProUGUI onlineText = default;
        
        protected override void OnAwake()
        {
            LobbyBus.CurrentOnlineCount.Subscribe(this, SetCount);
        }

		private void SetCount(int count) => onlineText.SetText($"Online: {count}");
    } 
}