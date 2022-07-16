using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class GameMaster : NetworkBehaviour
{
    public bool active { get; private set; } = false;
    public SSPState gameState = SSPState.Picking;
    
    private TextMeshProUGUI score;
    private GameObject resultCanvas;
    private NetworkManagerSSP networkManager;
    private int player1Score = 0, player2Score = 0;

    public enum SSPState
    {
        Picking, Countdown, Result
    }

    private void Awake()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerSSP>();
        score = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<TextMeshProUGUI>();

        if (NetworkClient.isHostClient)
        {
            resultCanvas = Instantiate(networkManager.spawnPrefabs[2]);
            NetworkServer.Spawn(resultCanvas);
            resultCanvas.SetActive(false);
            resultCanvas.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                RestartGame();
            });
        }
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

    [ServerCallback]
    public void NextRound()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        Player[] players = new Player[playerObjects.Length];
        for (int i = 0; i < playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].GetComponent<Player>();
        }


        Player.Pick player1Pick = players[0].currentPick;
        Player.Pick player2Pick = players[1].currentPick;

        if (player2Pick != player1Pick) {
            switch (player1Pick)
            {
                case Player.Pick.Schere:
                    if (player2Pick == Player.Pick.Papier)
                    {
                        player1Score++;
                    } else
                    {
                        player2Score++;
                    }
                    break;
                case Player.Pick.Stein:
                    if (player2Pick == Player.Pick.Schere)
                    {
                        player1Score++;
                    }
                    else
                    {
                        player2Score++;
                    }
                    break;
                case Player.Pick.Papier:
                    if (player2Pick == Player.Pick.Stein)
                    {
                        player1Score++;
                    }
                    else
                    {
                        player2Score++;
                    }
                    break;
            }
        }

        RpcUpdateScore(player1Score, player2Score);

        if (player1Score == 3 || player2Score == 3)
        {
            RpcSetState(SSPState.Result);
            resultCanvas.SetActive(true);
        } else
        {
            RpcSetState(SSPState.Picking);
        }
    }

    [ServerCallback]
    public void RestartGame()
    {
        RpcUpdateScore(0, 0);
        RpcSetState(SSPState.Picking);
        resultCanvas.SetActive(false);
    }

    [ClientRpc]
    private void RpcUpdateScore(int score1, int score2)
    {
        player1Score = score1;
        player2Score = score2;

        score.text = $"{score1} : {score2}";
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
