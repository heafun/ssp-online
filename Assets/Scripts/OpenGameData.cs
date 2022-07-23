using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Networking;

public class OpenGameData : MonoBehaviour
{
    [SerializeField]
    private TransferConnectInfo transferConnectInfo;
    [SerializeField]
    private ApiSettings apiSettings;
    [SerializeField]
    [Scene] private string playScene;

    public string objectId, ip, opponentName;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnJoinButtonClicked()
    {
        transferConnectInfo.ip = ip;
        transferConnectInfo.objectId = objectId;
        transferConnectInfo.networkMode = TransferConnectInfo.NetworkMode.Client;

        UnityWebRequest request = UnityWebRequest.Delete($"{apiSettings.apiBaseUrl}OpenGames/{objectId}");

        foreach (string header in apiSettings.headers)
        {
            string[] keyValuePair = header.Split(":");

            request.SetRequestHeader(keyValuePair[0], keyValuePair[1]);
        }

        request.SendWebRequest().completed += (AsyncOperation operation) =>
        {
            Debug.Log(request.downloadHandler.text);
            request.Dispose();
        };

        SceneManager.LoadScene(playScene);
    }
}
