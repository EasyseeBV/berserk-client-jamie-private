using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Lobby;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDrawerVersusLayout : MonoBehaviour, IProgressDrawerLayout
	{
		[SerializeField] protected ProgressDrawerPlayerLayout PlayerLayout1;
		[SerializeField] protected ProgressDrawerPlayerLayout PlayerLayout2;
		[SerializeField] protected VersusLayout VersusLayout;
		public virtual ProgressType Type => ProgressType.Versus;
		
		public virtual async UniTask ShowAsync(CancellationToken token = default, params object[] args)
		{
			await UniTask.WhenAll(SetupPlayers(args.OfType<LobbyPlayerModel>().ToArray(), token))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();
			
			if (token.IsCancellationRequested)
				return;
			
			gameObject.SetActive(true);
		}

		public UniTask CloseAsync(bool force = false, CancellationToken token = default, params object[] args)
		{
			if (token.IsCancellationRequested)
				return UniTask.CompletedTask;
			
			gameObject.SetActive(false);
			return UniTask.CompletedTask;
		}

		private UniTask SetupPlayers(IReadOnlyCollection<LobbyPlayerModel> players, CancellationToken token)
		{
			if (players.Count != 2)
				throw new ArgumentException("Players count are not equal player views.");
			
			return UniTask.WhenAll(players.OrderBy(x => x.UserId).Select((p, i) => SetupPlayer(p, i, token)))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();
		}

		private async UniTask SetupPlayer(LobbyPlayerModel playerModel, int playerIndex, CancellationToken token)
		{
			var view = GetPlayerView(playerIndex);
			await UniTask.WhenAll(
					view.SetArtMaskAsync("Vulcanite_Mask", token), 
					view.SetArtAsync(playerModel.AvatarUrl, token), 
					view.SetFrameArtAsync(playerModel.FrameUrl, token))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();
			
			if (token.IsCancellationRequested)
				return;

			VersusLayout.SetPlayerNameText(playerIndex, playerModel.UserName.Ellipsis(16));
			VersusLayout.SetPlayerMmrText(playerIndex, GetPlayerStatistic(playerModel.MMR));
		}
		
		private string GetPlayerStatistic(int? mmr)
		{
			return mmr.HasValue ? $"MMR: {mmr}" : null;
		}

		private ProgressDrawerPlayerLayout GetPlayerView(int playerIndex)
		{
			return playerIndex switch
			{
				0 => PlayerLayout1,
				1 => PlayerLayout2,
				_ => throw new NotImplementedException($"Unknown player index : {playerIndex}, maximum available players are 2.")
			};
		}
	}
}