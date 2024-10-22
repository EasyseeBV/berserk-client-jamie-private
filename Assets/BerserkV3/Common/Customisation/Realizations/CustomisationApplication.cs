using System;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.AudioSystem.Abstractions;
using RR.Core.Extensions;
using Zenject;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationApplication : ICustomisationApplication, IInitializable, IDisposable
	{
		private readonly IAudioApplication audioApplication;
		private readonly ICustomisationItemRepository itemRepository;
		private readonly ICustomisationItemFactory itemFactory;
		private readonly IGameDatabase gameDatabase;
		
		public CustomisationApplication(
			IAudioApplication audioApplication,
			ICustomisationItemRepository itemRepository, 
			ICustomisationItemFactory itemFactory,
			IGameDatabase gameDatabase)
		{
			this.audioApplication = audioApplication;
			this.itemRepository = itemRepository;
			this.itemFactory = itemFactory;
			this.gameDatabase = gameDatabase;
		}
		
		public void Initialize()
		{
			CustomisationBus.OnMusicPlayTest.SubscribeRaw(PlayTestMusic);
		}

		public async Task<bool> InitAsync()
		{
			if (itemRepository.Any())
			{
				ReInit();
				return true;
			}
			
			var itemModels = gameDatabase.AllCustomisations(); // get items from owned items from inventory when it will be required
			var equipped = await CustomisationAPI.GetUserEquippedUICustomizationsIds();
			if (!equipped)
			{
				equipped.Data = Array.Empty<string>();
				return false;
			}
			
			var items = itemModels.Select(x => itemFactory.Create(x, equipped.Data.Contains(x.Id)));
			itemRepository.AddRange(items);
			return true;
		}

		public async Task AcceptChanges()
		{
			var equippedItems = itemRepository.GetEquipped().Select(x=> x.Id).ToArray();
			await CustomisationAPI.PostEquippСustomizations(equippedItems);
			itemRepository.Save();
		}
		
		public void CancelChanges()
		{	
			itemRepository.Reset();
		}
		
		public void UnequipItemExept(string id, CustomisationType? type = null)
		{
			var itemType = type ?? itemRepository.Get().FirstOrDefault(x => x.Id.Same(id))?.CustomisationType;
			if (!itemType.HasValue)
				throw new InvalidOperationException($"Unknown {nameof(CustomisationType)} for id : {id}");
			
			itemRepository.Get(itemType.Value)
				.Where(x => !x.Id.Same(id))
				.ForEach(x => x.Unequip());
		}

		private void ReInit()
		{
			itemRepository.Get().ForEach(item => item.Clear());
		}

		public void UpdateMainThemeMusic(CustomisationType type)
		{
			var musicId = CustomisationServiceAdapter.Repository.GetFirstEquipped(type)?.PreviewURL;

			if (musicId == null || CustomisationBus.OnMusicPlayTest == musicId)
				return;
			
			CustomisationBus.OnMusicPlayTest.Assign(musicId);
			audioApplication.PlayMusic(musicId);
		}

		public void Dispose()
		{
			CustomisationBus.OnMusicPlayTest.Unsubscribe(PlayTestMusic);
		}

		private void PlayTestMusic(string musicId)
		{
			if (CustomisationBus.OnMusicPlayTest == musicId) 
				return;
			
			audioApplication.PlayMusic(musicId);
		}
	}
}