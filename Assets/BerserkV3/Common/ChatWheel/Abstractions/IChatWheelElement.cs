namespace BerserkV3.Generic.ChatWheel
{
	public interface IChatWheelElement
	{
		string Id { get; }

		IChatWheelSlot OccupiedSlot { get; }
		
		void Dispose();

		/// <summary>
		/// Hold target Element by other parent
		/// </summary>
		/// <param name="parent"></param>
		void Hold(IChatWheelSlot parent);

		/// <summary>
		/// Release element from any holders
		/// </summary>
		void Release();
	}
}