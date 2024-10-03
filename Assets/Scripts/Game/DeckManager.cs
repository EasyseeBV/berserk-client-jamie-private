using UnityEngine;

namespace Game
{
	// TODO remove legacy
	public class DeckManager : MonoBehaviour
	{
		// public bool Debug;
		//
		// // todo: to settings
		// [Header("Cards in hand setting"), SerializeField]
		// private float cardHandOffsetY = 36f;
		//
		// [SerializeField] private Vector2 cardArcSize = new Vector2(300, 200);
		//
		// [SerializeField] private RectTransform playerHandContent;
		// [SerializeField] private RectTransform opponentHandContent;
		// [SerializeField, AssetsOnly] private HandCardEntity playerHandCardPrefab;
		// [SerializeField, AssetsOnly] private HandCardEntity opponentHandCardPrefab;
		//
		// private static DeckManager instance;
		//
		// private void CreateInstance()
		// {
		// 	// if (instance == null)
		// 	// 	instance = this;
		// 	// else if (instance != this)
		// 	// 	Destroy(gameObject);
		// }
		//
		// protected void Awake()
		// {
		// 	// CreateInstance();
		// 	//
		// 	// if (EnvironmentSwitcher.CurrentEnvironment <= Environment.Staging)
		// 	// 	RRConsole.AddCommand(nameof(AddCardById), AddCardById,
		// 	// 		$"Adds specific card. {{id}} {string.Join(",", Enum.GetNames(typeof(Owner)))}");
		// 	//
		// 	// RRConsole.AddCommand("Debug", ToggleDebugMode, "Toggles debug mode");
		// 	//
		// 	// DeleteAllCards(Owner.Self);
		// 	// DeleteAllCards(Owner.Opponent);
		// 	//
		// 	// GameBus.SpawnCardsInHand.Subscribe(this, x => x != null && x.Any(), SpawnCardsInHand);
		// 	// GameBus.OnRequestHandRearrange.Subscribe(this, RearrangeHand);
		// }
		//
		// // private void SpawnCardsInHand(params CardData[] cardsData)
		// // {
		// // 	// cardsData.ForEach(data => AddCardInHand(data, data.Owner));
		// // 	//
		// // 	// RearrangeHand(cardsData.First().Owner);
		// // 	//
		// // 	// void AddCardInHand(CardData data, Owner owner)
		// // 	// {
		// // 	// 	if (owner == Owner.None)
		// // 	// 	{
		// // 	// 		RRLogger.Warning($"Add Card in Hand owner is None, add cards to both players Card : {data}");
		// // 	// 		AddCardInHand(data, Owner.Self);
		// // 	// 		AddCardInHand(data, Owner.Opponent);
		// // 	// 		return;
		// // 	// 	}
		// // 	//
		// // 	// 	var handCardPrefab = owner == Owner.Self
		// // 	// 		? playerHandCardPrefab
		// // 	// 		: opponentHandCardPrefab;
		// // 	//
		// // 	// 	var handContent = owner == Owner.Self
		// // 	// 		? playerHandContent
		// // 	// 		: opponentHandContent;
		// // 	//
		// // 	// 	var handCardEntity = Instantiate(handCardPrefab, handContent);
		// // 	//
		// // 	// 	data.Owner = owner;
		// // 	// 	handCardEntity.Init(data);
		// // 	// 	handCardEntity.name = data.Title + " [HandCard]";
		// // 	// 	//Context change
		// // 	// 	GameBus.LocalContext.AddHandCard(handCardEntity);
		// // 	// }
		// // }
		//
		// private void RearrangeHand(Owner owner)
		// {
		// 	// var isPlayer = owner == Owner.Self;
		// 	// var handContent = isPlayer ? playerHandContent : opponentHandContent;
		// 	// var ownerFactor = isPlayer ? 1 : -1;
		// 	//
		// 	// var playerCards = GameBus.LocalContext
		// 	// 	.GetHandCardsByOwner(owner)
		// 	// 	.Select(c => c.transform)
		// 	// 	.ToList();
		// 	//
		// 	// var pivot = new Vector3(handContent.position.x, cardHandOffsetY * ownerFactor + handContent.position.y,
		// 	// 	handContent.position.z);
		// 	//
		// 	// float GetArcPosition(int index, int length)
		// 	// {
		// 	// 	return (-index + (length - 1) / 2f) * 8;
		// 	// }
		// 	//
		// 	// for (var i = 0; i < playerCards.Count; i++)
		// 	// {
		// 	// 	var angle = GetArcPosition(i, playerCards.Count);
		// 	// 	var x = -Mathf.Sin(angle * Mathf.PI / 180f) * cardArcSize.x;
		// 	// 	var y = ownerFactor * Mathf.Cos(angle * Mathf.PI / 180f) * cardArcSize.y;
		// 	//
		// 	// 	var card = playerCards[i];
		// 	// 	var position = pivot + new Vector3(x, y, 0);
		// 	// 	var rotation = Quaternion.Euler(0, 0, ownerFactor * angle);
		// 	//
		// 	// 	if (Application.isPlaying)
		// 	// 	{
		// 	// 		card.transform.DOLocalMove(position, 0.33f);
		// 	// 		card.transform.DOLocalRotateQuaternion(rotation, 0.33f);
		// 	// 	}
		// 	// 	else
		// 	// 	{
		// 	// 		card.transform.localPosition = position;
		// 	// 		card.transform.localRotation = rotation;
		// 	// 	}
		// 	//
		// 	// 	card.transform.SetAsLastSibling();
		// 	// 	card.transform.hasChanged = true;
		// 	//
		// 	// 	if (isPlayer)
		// 	// 		card.GetComponent<HandCardView>().SetPosition(position, rotation);
		// 	// }
		// 	//
		// 	// AudioController.Play(Clip.Card_Spawn);
		// 	// RRLogger.Log($"[{"Logic".Green().Bold()}] Rearranged cards of {owner}:{playerCards.Count}".Gray());
		// }
		//
		// [Button]
		// public static void DeleteAllCards(Owner owner)
		// {
		// 	// switch (owner)
		// 	// {
		// 	// 	case Owner.None:
		// 	// 		while (instance.playerHandContent.childCount > 0)
		// 	// 			instance.playerHandContent.DestroyChildrenImmediate();
		// 	//
		// 	// 		while (instance.opponentHandContent.childCount > 0)
		// 	// 			instance.opponentHandContent.DestroyChildrenImmediate();
		// 	// 		break;
		// 	//
		// 	// 	case Owner.Self:
		// 	// 		while (instance.playerHandContent.childCount > 0)
		// 	// 			instance.playerHandContent.DestroyChildrenImmediate();
		// 	// 		break;
		// 	//
		// 	// 	case Owner.Opponent:
		// 	// 		while (instance.opponentHandContent.childCount > 0)
		// 	// 			instance.opponentHandContent.DestroyChildrenImmediate();
		// 	// 		break;
		// 	//
		// 	// 	default:
		// 	// 		throw new ArgumentOutOfRangeException(nameof(owner), owner, null);
		// 	// }
		// }
		//
		// private string AddCardById(string[] args)
		// {
		// 	// if (!args.Any())
		// 	// 	return
		// 	// 		"Provide card id as a parameter";
		// 	//
		// 	// AddCardById(args[0]);
		// 	// return $"Card added to {Owner.Self}";
		// 	return string.Empty;
		// }
		//
		// [Button(ButtonSizes.Medium)]
		// private void AddCardById(string cardId)
		// {
		// 	// var newCard = GameDataBaseAdapter.Instance.GetCard(cardId);
		// 	// newCard.Owner = Owner.Self;
		// 	// newCard.IsSpawnedByEffect = true;
		// 	// newCard.Lava.Set(0);
		// 	//
		// 	// SpawnCardsInHand(newCard);
		// 	//
		// 	// RearrangeHand(Owner.Self);
		// 	// RRLogger.Log($"[{"Command".Brown().Bold()}] Added card \n{JsonConvert.SerializeObject(newCard).Teal()}");
		// }
		//
		// private string ToggleDebugMode(string[] args)
		// {
		// 	// Debug = args.Length < 1
		// 	//         || args[0].ToLower() == "true"
		// 	//         || args[0].ToLower() == "on"
		// 	//         || args[0] == "1";
		// 	//
		// 	// return $"Debug mode {(Debug ? "ON".Green() : "OFF".Red())}";
		// 	return string.Empty;
		// }
		//
		// public static void Dispose()
		// {
		// 	// if (instance)
		// 	// 	Destroy(instance.gameObject);
		// 	//
		// 	// instance = null;
		// }
	}
}