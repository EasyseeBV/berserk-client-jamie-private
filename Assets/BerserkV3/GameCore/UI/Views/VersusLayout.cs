using System;
using RR.UI.FrameSystem;

namespace BerserkV3.GameCore.UI
{
	public partial class VersusLayout : BaseView
	{
		public void SetPlayerNameText(int playerIndex, string value)
		{
			switch (playerIndex)
			{
				case 0 : SetPlayer1NameText(value);
					break;
				
				case 1 : SetPlayer2NameText(value);
					break;
				
				default: throw new NotImplementedException($"Unknown player index : {playerIndex}, maximum available players are 2.");
			}
		}
		
		public void SetPlayerMmrText(int playerIndex, string value)
		{
			switch (playerIndex)
			{
				case 0 : SetPlayer1MmrText(value);
					break;
				
				case 1 : SetPlayer2MmrText(value);
					break;
				
				default: throw new NotImplementedException($"Unknown player index : {playerIndex}, maximum available players are 2.");
			}
		}
		
		public void SetPlayer1NameText(string value)
		{
			if (Player1NameText)
				Player1NameText.SetText(value ?? "Unknown");
		}

		public void SetPlayer2NameText(string value)
		{
			if (Player2NameText)
				Player2NameText.SetText(value ?? "Unknown");
		}

		public void SetPlayer1MmrText(string value)
		{
			if (!Player1MMRText)
				return;
			Player1MMRText.SetText(value);
			SetActive(Player1MMRText, !string.IsNullOrEmpty(value));
		}

		public void SetPlayer2MmrText(string value)
		{
			if (!Player2MMRText)
				return;

			Player2MMRText.SetText(value);
			SetActive(Player2MMRText, !string.IsNullOrEmpty(value));
		}
	}
}