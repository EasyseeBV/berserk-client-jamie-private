using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions;
using BerserkV3.Lobby.UI.Home.General;
using BerserkV3.Lobby.Wallets.Abstractions;
using BerserkV3.Lobby.Wallets.Realizations;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.UIService;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets.Controllers
{
    public class CurrencyPanelPresenter : ICurrencyPanelPresenter, IDisposable
    {
	    public event Action OnWalletClicked;
	    
        private const string LavaSymbol = "<sprite name=\"LavaGemIcon\">";
        private const string CoinSymbol = "<sprite name=\"GoldCoinIcon\">";

        private readonly IUIService uiService;
        private readonly IWalletsApplication walletsApplication;
        private readonly IWalletsRepository walletsRepository;

        private CurrencyPanel currencyPanel;
        private Action walletsFetchedHandler;

        public CurrencyPanelPresenter(
            IUIService uiService,
            IWalletsApplication walletsApplication,
            IWalletsRepository walletsRepository)
        {
            this.uiService = uiService;
            this.walletsApplication = walletsApplication;
            this.walletsRepository = walletsRepository;
        }

        public void Init()
        {
            InitAsync().Forget();
        }

        public void Show()
        {
            var window = uiService.Get<GeneralWindow>();
            if (window == null)
            {
                RRLogger.Error("GeneralWindow not found. Cannot display the currency panel.");
                return;
            }

            currencyPanel = window.CurrencyPanel;
            if (currencyPanel == null)
            {
                RRLogger.Error("CurrencyPanel not found in GeneralWindow.");
                return;
            }

            currencyPanel.SetActive(true);
        }

        public void Hide()
        {
            currencyPanel?.SetActive(false);
        }

        private async UniTask InitAsync()
        {
            try
            {
                currencyPanel?.SetActive(false);

                walletsApplication.OnWalletsFetchFailed += HandleFetchError;
                walletsApplication.OnWalletBalanceChanged += HandleWalletBalanceUpdated;

                var isWalletsReady = await walletsApplication.TryFetchPlayerWalletsAsync();
                if (isWalletsReady)
                    UpdateContent();
                else
                    HandleFetchError();

                walletsFetchedHandler = UpdateContent;
                walletsApplication.OnWalletsFetched += walletsFetchedHandler;
            }
            catch (Exception ex)
            {
                RRLogger.Error($"An error occurred during initialization: {ex.Message}");
            }
        }

        private void UpdateContent()
        {
            if (currencyPanel == null)
            {
                RRLogger.Error($"{nameof(UpdateContent)} failed. CurrencyPanel is not initialized.");
                return;
            }

            var wallets = walletsRepository.GetAll();

            // MOCK CODE until server properly sends wallets
            if (wallets.Count == 0)
            {
                wallets = new Dictionary<WalletType, IPlayerWallet>
                {
                    { WalletType.AesSedaiGems, new PlayerWallet(WalletType.AesSedaiGems, 12405037, true) },
                    { WalletType.AetherCoins, new PlayerWallet(WalletType.AetherCoins, 545000, true) },
                };
            }
            // MOCK CODE end

            if (wallets.Count == 0)
            {
                currencyPanel.SetActive(false);
                RRLogger.Error($"{nameof(UpdateContent)} failed. Wallets list is empty");
                return;
            }

            currencyPanel.SetActive(true);

            var widgets = currencyPanel.Widgets;
            var walletTypes = wallets.Keys.ToList();

            UIHelper.InitWidgets(widgets, walletTypes.Count, (widget, index) =>
            {
                var walletType = walletTypes[index];
                var wallet = wallets[walletType];

                widget.Init(walletType, OnWalletClicked);
                widget.SetAmount(wallet.Balance);
                widget.SetIcon(GetIconForWalletType(walletType));
            });
        }

        private void HandleFetchError()
        {
            currencyPanel?.SetActive(false);
            if (walletsFetchedHandler != null)
                walletsApplication.OnWalletsFetched -= walletsFetchedHandler;
            RRLogger.Error($"{nameof(HandleFetchError)}: Wallets fetch failed. The currency panel has been deactivated.");
        }

        private void HandleWalletBalanceUpdated(WalletType type, int newAmount)
        {
            if (currencyPanel == null)
            {
                RRLogger.Error($"{nameof(HandleWalletBalanceUpdated)} failed. CurrencyPanel is not initialized.");
                return;
            }

            var widget = currencyPanel.Widgets.FirstOrDefault(w => w.WalletType == type);
            if (widget == null)
            {
                RRLogger.Error($"{nameof(HandleWalletBalanceUpdated)} failed. Widget with type {type} not found.");
                return;
            }

            widget.SetAmount(newAmount);
        }

        private string GetIconForWalletType(WalletType walletType)
        {
            return walletType switch
            {
                WalletType.AetherCoins => CoinSymbol,
                WalletType.AesSedaiGems => LavaSymbol,
                _ => string.Empty
            };
        }

        public void Dispose()
        {
            walletsApplication.OnWalletsFetchFailed -= HandleFetchError;
            walletsApplication.OnWalletBalanceChanged -= HandleWalletBalanceUpdated;
            if (walletsFetchedHandler != null)
                walletsApplication.OnWalletsFetched -= walletsFetchedHandler;
            currencyPanel?.Clear();
        }
    }
}
