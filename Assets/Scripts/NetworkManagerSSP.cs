using UnityEngine;
using Mirror;

public class NetworkManagerSSP : NetworkManager
{
    [SerializeField]
    private Transform spawn1, spawn2;
    [SerializeField]
    public GameObject selectMenu;

    private GameMaster gameMaster;

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

            gameMaster = Instantiate(spawnPrefabs[0]).GetComponent<GameMaster>();
            NetworkServer.Spawn(gameMaster.gameObject);
            gameMaster.RpcSetActive(true);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        if (gameMaster != null)
        {
            NetworkServer.UnSpawn(gameMaster.gameObject);
        }
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        gameMaster = null;
    }

    private void Update()
    {
        int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;

        if (playerCount == 2 && gameMaster == null)
        {
            gameMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMaster>();
        }

        if (playerCount == 2 && !selectMenu.activeInHierarchy && gameMaster.gameState == GameMaster.SSPState.Picking)
        {
            selectMenu.SetActive(true);
        } else if((playerCount != 2 || gameMaster.gameState != GameMaster.SSPState.Picking) && selectMenu.activeInHierarchy)
        {
            selectMenu.SetActive(false);
        }
    }

    [Client]
    public void Pick(int pick)
    {
        NetworkClient.localPlayer.GetComponent<Player>().CmdPick(pick);
    }
}
