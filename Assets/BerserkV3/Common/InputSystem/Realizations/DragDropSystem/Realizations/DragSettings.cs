namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public class DragSettings : IDragSettings
	{
		public bool Enable {get; set;}
		public bool ResetOnCanceled {get; set;}
		public bool InOutTransparency {get; set;}
		public float TransparencyAmount {get; set;}
		public bool WorldPositionStaysWhenDraggingStart {get; set;}
		public bool WorldPositionStaysWhenDraggingCanceled {get; set;}

		public DragSettings()
		{
			ApplyDefaultValues();
		}
        
		public static DragSettings Default()
		{
			return new DragSettings().ApplyDefaultValues();
		}
        
		private DragSettings ApplyDefaultValues()
		{
			Enable = true;
			ResetOnCanceled = true;
			InOutTransparency = true;
			TransparencyAmount = 1.25f;
			WorldPositionStaysWhenDraggingStart = false;
			WorldPositionStaysWhenDraggingCanceled = false;
			return this;
		}
	}
}