using System;
using System.Collections;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class QuadrantView : MonoBehaviour
{
	[SerializeField] private Button btn;
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text subtitleText;
	[SerializeField] private TMP_Text rewardText;
	[SerializeField] private RawImage background1;
	[SerializeField] private RawImage background2;
	[SerializeField] private RawImage iconTop;
	[SerializeField] private RawImage iconEmblem;
	public void SetBaseQuadrant(CampaignQuadrantDefinition quadrant)
	{
		titleText.text = quadrant.DisplayName;
		subtitleText.text = quadrant.Subtitle;
		rewardText.text = quadrant.RewardText;
		background1.texture = Resources.Load<Texture2D>(quadrant.PreviewTextureResource);
		background2.texture = background1.texture;
		if(iconTop != null)
			iconTop.texture = Resources.Load<Texture2D>(quadrant.TopIconTextureResource);
		if (iconEmblem != null)
			iconEmblem.texture = Resources.Load<Texture2D>(quadrant.EmblemTextureResource);

	}

	public void SetSubtitle(string txt) => subtitleText.text = txt;
	public void SetReward(string txt) => rewardText.text = txt;

	public void SetBackgroundCol(Color color)
	{
		background2.color = color;
	}

	public void SetButtonAcion(UnityAction action, bool interactive)
	{
		btn.interactable = interactive;
		btn.onClick.AddListener(action);
	}
}
