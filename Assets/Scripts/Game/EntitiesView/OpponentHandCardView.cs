using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using Vulcan.Data;

namespace UI
{
	public partial class OpponentHandCardView : MonoEntityBaseView
	{
		private CancellationTokenSource imageLoading;

		public override void SetUp(DataBase data)
		{
			var cardData = data as CardData;
			imageLoading?.Cancel();
			imageLoading?.Dispose();
			imageLoading = new CancellationTokenSource();
			CardShirtImage.LoadResourceAsync(cardData?.BackArtUrl, imageLoading.Token).Forget();
		}

		private void OnDestroy()
		{
			CardShirtImage.ReleaseResource();
		}

	#region Not Implemented
		public override void Select(Color selectedColor){}

		public override void Unselect(){}

		public override void SelectOnRequestingTarget(Color requestingColor){}

		public override void SetPosition(Vector3 position, Quaternion quaternion){}
	#endregion
		
	}
}
