using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ConnectInfoTransfer", menuName ="ScriptableObject/ConnectInfoTransfer")]
public class TransferConnectInfo : ScriptableObject
{
    public string ip;
    public NetworkMode networkMode;
    public string objectId;

    public enum NetworkMode
    {
        Host, Client
    }
}
