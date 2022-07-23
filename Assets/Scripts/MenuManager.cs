using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mirror;
using UnityEngine.SceneManagement;

public class OpenGamesObject
{
    public string objectId, hostAddress, createdAt, updatedAt;
}

public class ResponseObject
{
    public string objectId, createdAt;
}

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject addGameButton, scrollViewContent, OpenGameButtonPrefab;
    [SerializeField]
    private TransferConnectInfo transferConnectInfo;
    [SerializeField]
    [Scene] private string playScene;
    [SerializeField]
    private ApiSettings apiSettings;

    private Coroutine updateGameListRoutine;

    private void Start()
    {
        addGameButton.GetComponent<Button>().onClick.AddListener(createGame);
        updateGameListRoutine = StartCoroutine(UpdateGameList());
    }

    private void createGame()
    {
        UnityWebRequest request = new UnityWebRequest($"{apiSettings.apiBaseUrl}OpenGames", "POST");
        string localIp = GetLocalIPAddress();
        byte[] bodyRaw = Encoding.UTF8.GetBytes($"{{ \"hostAddress\" : \"{localIp}\" }}");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        foreach (string header in apiSettings.headers)
        {
            string[] keyValuePair = header.Split(":");

            request.SetRequestHeader(keyValuePair[0], keyValuePair[1]);
        }

        request.SendWebRequest().completed += (AsyncOperation operation) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseObject response = JsonUtility.FromJson<ResponseObject>(request.downloadHandler.text);

                transferConnectInfo.objectId = response.objectId;
                transferConnectInfo.ip = localIp;
                transferConnectInfo.networkMode = TransferConnectInfo.NetworkMode.Host;

                StopCoroutine(updateGameListRoutine);

                SceneManager.LoadScene(playScene);
            } else
            {
                Debug.Log(request.error);
            }
            request.Dispose();
        };
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) //InterNetworkV6 for Ipv6 Adress
            {
                return ip.ToString();
            }
        }
        return null;
    }

    private IEnumerator UpdateGameList()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{apiSettings.apiBaseUrl}OpenGames");

            foreach (string header in apiSettings.headers)
            {
                string[] keyValuePair = header.Split(":");

                request.SetRequestHeader(keyValuePair[0], keyValuePair[1]);
            }

            request.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                string result = request.downloadHandler.text;
                request.Dispose();
                result = result.Split("[")[1];
                result = result.Split("]")[0];

                string[] openGameList = result.Split("{");

                ArrayList openGames = new ArrayList();

                for(int i=0; i<openGameList.Length; i++)
                {
                    if (openGameList[i].Length==0)
                    {
                        continue;
                    }
                    if (openGameList[i][0]!='{')
                    {
                        openGameList[i] = "{" + openGameList[i];
                    }
                    if (openGameList[i].EndsWith(','))
                    {
                        openGameList[i] = openGameList[i].Remove(openGameList[i].Length-1);
                    }
                    openGames.Add(JsonUtility.FromJson<OpenGamesObject>(openGameList[i]));
                }

                OpenGameData[] currentOpenGameData = scrollViewContent.GetComponentsInChildren<OpenGameData>();

                foreach(OpenGameData openGameData in currentOpenGameData) //Destroy OpenGames that have been closed
                {
                    bool contains = false;
                    foreach (OpenGamesObject openGame in openGames)
                    {
                        contains = contains || openGameData.objectId == openGame.objectId; 
                    }
                    if (!contains)
                    {
                        Destroy(openGameData.gameObject);
                    }
                }

                foreach (OpenGamesObject openGame in openGames) // Add all new created Games to the List
                {
                    bool contains = false;
                    foreach (OpenGameData openGameData in currentOpenGameData)
                    {
                        contains = contains || openGameData.objectId == openGame.objectId;
                    }
                    if (!contains)
                    {
                        OpenGameData newGameData = Instantiate(OpenGameButtonPrefab, scrollViewContent.transform).GetComponent<OpenGameData>();
                        newGameData.objectId = openGame.objectId;
                        newGameData.ip = openGame.hostAddress;
                        newGameData.opponentName = "TestName"; //TODO
                    }
                }
            };


            yield return new WaitForSeconds(3f);
        }
    }
}
