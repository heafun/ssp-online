using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private Sprite schere, stein, papier;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    [Command]
    public void CmdPick(int pick)
    {
        DisplayPick(pick);
    }

    [ClientRpc]
    public void DisplayPick(int pick)
    {
        switch (pick)
        {
            case 0:
                spriteRenderer.sprite = schere;
                break;
            case 1:
                spriteRenderer.sprite = stein;
                break;
            case 2:
                spriteRenderer.sprite = papier;
                break;
        }
    }

    [ClientRpc]
    public void FlipX(bool flip)
    {
        spriteRenderer.flipX = true;
    }
}
