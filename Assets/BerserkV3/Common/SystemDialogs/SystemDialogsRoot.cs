using BerserkV3.Common.ProgressDrawer;
using BerserkV3.GameCore.UI;
using RR.Core.Components;
using UnityEngine;

namespace BerserkV3.Generic.SystemDialogs
{
	public class SystemDialogsRoot : Singleton<SystemDialogsRoot>
	{
		[SerializeField] private ReconnectionView reconnectionView;
		[SerializeField] private ProgressDrawerView progressDrawerView;
		
		public IProgressDrawerView ProgressDrawerView => progressDrawerView;
		public IReconnectionView ReconnectionView => reconnectionView;
	}
}