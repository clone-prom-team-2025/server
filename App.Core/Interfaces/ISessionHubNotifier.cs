namespace App.Core.Interfaces;

public interface ISessionHubNotifier
{
    Task ForceLogoutAsync(string sessionId);
}