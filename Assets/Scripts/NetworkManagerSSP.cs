using UnityEngine;
using Mirror;

public class NetworkManagerSSP : NetworkManager
{
    [SerializeField]
    private Transform spawn1, spawn2;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // add player at correct spawn position
        Transform spawn;

        if (numPlayers==0)
        {
            spawn = spawn1;
        } else
        {
            spawn = spawn2;
        }

        GameObject newPlayer = Instantiate(playerPrefab, spawn.position, spawn.rotation);
        NetworkServer.AddPlayerForConnection(conn, newPlayer);

        if (numPlayers == 2)
        {
            newPlayer.GetComponent<Player>().FlipX(true);
        }
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
    }

    [Client]
    public void Pick(int pick)
    {
        NetworkClient.localPlayer.GetComponent<Player>().CmdPick(pick);
    }
}
