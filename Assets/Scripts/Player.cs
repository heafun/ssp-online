using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private Sprite schere, stein, papier;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public Pick currentPick { get; private set; } = Pick.Stein;
    public bool ready = false;

    public enum Pick
    {
        Schere, Stein, Papier
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }


    [Command]
    public void CmdPick(int pick)
    {
        Pick pickEnum = Pick.Stein;
        switch (pick)
        {
            case 0:
                pickEnum = Pick.Schere;
                break;
            case 2:
                pickEnum = Pick.Papier;
                break;
        }

        RpcPick(pickEnum);
    }

    [ClientRpc]
    public void RpcPick(Pick pick)
    {
        currentPick = pick;
        ready = true;
    }

    [ClientRpc]
    public void RpcStartCountdown()
    {
        animator.SetTrigger("Count");
        ready = false;
    }

    public void RevealPick()
    {
        switch (currentPick)
        {
            case Pick.Schere:
                spriteRenderer.sprite = schere;
                break;
            case Pick.Stein:
                spriteRenderer.sprite = stein;
                break;
            case Pick.Papier:
                spriteRenderer.sprite = papier;
                break;
        }

        if (NetworkClient.isHostClient && isLocalPlayer)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameMaster>().NextRound();
        }
    }

    [ClientRpc]
    public void FlipX(bool flip)
    {
        spriteRenderer.flipX = flip;
    }
}
