using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameMaster : NetworkBehaviour
{
    public bool active { get; private set; } = false;
    public SSPState gameState = SSPState.Picking;

    private NetworkManagerSSP networkManager;

    public enum SSPState
    {
        Picking, Countdown
    }

    private void Awake()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerSSP>();
    }

    [ServerCallback]
    private void Update()
    {
        if (!active)
            return;

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Player[] players = new Player[playerObjects.Length];
        for (int i=0; i<playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].GetComponent<Player>();
        }
        if (players.Length==2)
        {
            switch (gameState)
            {
                case SSPState.Picking:
                    if (players[0].ready && players[1].ready)
                    {
                        RpcSetState(SSPState.Countdown);
                        for (int i = 0; i < players.Length; i++)
                        {
                            players[i].RpcStartCountdown();
                        }
                    }
                    break;
            }
        }
    }

    [ClientRpc]
    public void RpcSetState(SSPState state)
    {
        gameState = state;
    }

    [ClientRpc]
    public void RpcSetActive(bool state)
    {
        active = state;
        gameState = SSPState.Picking;
    }

}
