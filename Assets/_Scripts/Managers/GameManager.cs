using Unity.Netcode;

public class GameManager : NetworkBehaviour {
    
    public override void OnDestroy() {
        base.OnDestroy();
        MatchmakingService.LeaveLobby();
        if (NetworkManager.Singleton != null )NetworkManager.Singleton.Shutdown();
    }

}