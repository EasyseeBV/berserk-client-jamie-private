using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.UIKit;
using Global;
using RR.Core.Extensions;
using UnityEngine;

public class SearchCardsView : View
{
	[SerializeField]
	private TextInputField searchInput;

	public event Action OnFilterChanged;

	public Func<CardData, bool> FilterFunction { get; private set; } = _ => true;

	private string lastQuery = string.Empty;

	protected override void OnAwake()
	{
		if (searchInput == null)
			searchInput = GetComponent<TextInputField>();
		searchInput.onValueChanged.AddListener(UpdateFilter);
	}

	protected override void OnShown() => OnFilterChanged?.Invoke();

	protected override void OnClosed()
	{
		base.OnClosed();
		lastQuery = string.Empty;
		Set(searchInput, string.Empty);
	}
	protected override void OnHidden()
	{
		base.OnClosed();
		lastQuery = string.Empty;
		Set(searchInput, string.Empty);
	}

	private readonly Regex trimPattern = new("[;,\t\r\n_ ]", RegexOptions.IgnoreCase);
	
	private void UpdateFilter(string query)
	{
		query = query.ToLower();

		if (lastQuery.Same(query))
			return;

		lastQuery = query;

		if (string.IsNullOrEmpty(query))
			FilterFunction = _ => true;
		else
			FilterFunction = FilterbyCard;

		OnFilterChanged?.Invoke();

		bool FilterbyCard(CardData card)
		{
			return FilterElement(card.Artist)
			       || FilterElement(card.Description)
			       || FilterElement(card.Id)
			       || FilterElement(card.Title)
			       || FilterElement(card.Tooltip)
			       || FilterElement(card.ArtUrl)
			       
			       || FilterElementBy(card.EffectsIds)
			       || FilterElementBy(card.InvalidActions)
			       || FilterElementBy(card.Factions)
			       || FilterElementBy(card.Races)
			       || FilterElementBy(card.SubTypes)
			       
			       || FilterElement(card.Attack)
			       || FilterElement(card.Hp)
			       || FilterElement(card.Mana)
			       || FilterElement(card.Quadrant)
			       || FilterElement(card.Rarity)
			       || FilterElement(card.Season)
			       || FilterElement(card.Type)
			       || FilterElement(card.ArtType);
		}

		bool FilterElementBy<T>(IEnumerable<T> value)
		{
			return value != null && value.Any(x => FilterElement(x));
		}
		
		bool FilterElement(object data)
		{
			var value = data?.ToString();
			return !string.IsNullOrEmpty(value)
			       && TrimFormat(value).Contains(TrimFormat(query));
		}
		
		string TrimFormat(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;
			
			return trimPattern.Replace(value, "").ToLower();
		}
	}
}