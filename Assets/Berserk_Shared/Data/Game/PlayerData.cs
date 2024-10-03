namespace Berserk.Shared.Data.Game
{
    public class PlayerData
    {
        public bool IsBot { get; set; }
        public bool FirstMover { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int? MMR { get; set; }
    
        public PlayerDeckModel CurrentDeck { get; set; }
    }
}