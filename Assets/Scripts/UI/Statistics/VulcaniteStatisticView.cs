using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class VulcaniteStatisticView : BaseStatisticView
	{
		public RawImage BorderRawImage => BorderImage;
		
		public override RawImage RawImage => Image;

		public override RawImage RawImageMask => Mask;

		public override Image BackgroundUnderWhole => WholeBackground;

		public override Image BackgroundUnderText => TextBackground;
		
		public override TextMeshProUGUI StatTextKey => TextKey;
		
		public override TextMeshProUGUI StatTextValue => TextValue;
		
		public override GameObject RawImageLayout => ImageLayout.gameObject;

		public void SetActiveBorderImage(bool value)
		{
			SetActive(BorderRawImage, value);
		}
	}
}