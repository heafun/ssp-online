using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClickHandler : MonoBehaviour
{
    [SerializeField]
    private NetworkManagerSSP networkManager;

    private void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerSSP>();
        gameObject.SetActive(false);
    }

    public void Pick(int pick)
    {
        networkManager.Pick(pick);
    }
}
