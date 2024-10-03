using System.Threading.Tasks;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Network.Enums;
using RR.UI.FrameSystem;

namespace BerserkV3.Startup.UI
{
	public partial class ServerChoiceView : BaseView
	{
		private TaskCompletionSource<object> wait;
		private static string lastCustomUrl = "https://ccg-berserk-dev-v4.azurewebsites.net/";

		protected override void OnAwake()
		{
			SetActive(InputPage, false);
			SetActive(MainPage, true);
			Subscribe(StageButton, () => SetEnvironment(Environment.Staging));
			Subscribe(ProdButton, () => SetEnvironment(Environment.Production));
			Subscribe(PublicButton, () => SetEnvironment(Environment.Test));
			Subscribe(LocalhostButton, () => SetEnvironment(Environment.LocalHost));
			Subscribe(DevButton, () => SetEnvironment(Environment.Development));

			// Custom Input
			Subscribe(CustomButton, () => SetActive(MainPage, false));
			Subscribe(CustomButton, () => SetActive(InputPage, true));
			Subscribe(CustomButton, () => SetEnvironment(Environment.Custom, selected: false));
			Subscribe(InputSubmitButton, () => lastCustomUrl = CusomInput.text);
			Subscribe(InputSubmitButton, () => URLs.SetCustomServerUrl(Region.US, CusomInput.text));
			Subscribe(InputSubmitButton, () => URLs.SetCustomServerUrl(Region.EU, CusomInput.text));
			Subscribe(InputSubmitButton, OnSelected);
		}

		private void OnEnable()
		{
			if (wait == null && VisibleState != VisibleState.Visible)
				gameObject.SetActive(false);
		}

		public async Task Init()
		{
			wait = new TaskCompletionSource<object>();
			CusomInput.text = lastCustomUrl;
			Show();
			await wait.Task;
		}

		private void OnSelected()
		{
			wait?.SetResult(null);
			Close();
			wait = null;
		}

		private void SetEnvironment(Environment environment, bool selected = true)
		{
			EnvironmentSwitcher.SwitchEnvironment(environment);
			if (selected)
				OnSelected();
		}
	}
}