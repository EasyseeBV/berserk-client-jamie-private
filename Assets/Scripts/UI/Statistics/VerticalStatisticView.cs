using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class VerticalStatisticView : BaseStatisticView
	{
		public override RawImage RawImage => Image;

		public override RawImage RawImageMask => Mask;

		public override Image BackgroundUnderWhole => WholeBackground;

		public override Image BackgroundUnderText => TextBackground;
		
		public override TextMeshProUGUI StatTextKey => TextKey;
		
		public override TextMeshProUGUI StatTextValue => TextValue;
		
		public override GameObject RawImageLayout => ImageLayout.gameObject;
	}
}