using System.Collections.Generic;
using BerserkV3.Lobby.UI.General.Profile.MatchLog;
using RR.Core.Extensions;
using RR.UIService;
using UnityEngine;
using UnityEngine.Serialization;

namespace BerserkV3.Lobby.UI.General.Profile.Collection
{
	public class ProfileCollectionWindow : UIWindowBase
	{
		[SerializeField] private CollectionRowWidget profileCollectionRow;
		[SerializeField] private Transform contentParent;
		
		public CollectionRowWidget CreateRow()
		{
			return Instantiate(profileCollectionRow, contentParent);
		}
		
		public void Clear()
		{
			contentParent.DestroyChildren();
		}
	}
}