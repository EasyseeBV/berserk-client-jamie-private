namespace Berserk.Shared.SignalR.Lobby
{
    public class RefreshTokenSignalDto
    {
        public string NotifyMessage { get; }
        public string RefreshToken { get; }

        public RefreshTokenSignalDto(string notifyMessage, string refreshToken)
        {
            NotifyMessage = notifyMessage;
            RefreshToken = refreshToken;
        }
    }
}