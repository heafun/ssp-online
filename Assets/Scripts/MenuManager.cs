using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class OpenGamesObject
{
    public string objectId, hostAddress, createdAt, updatedAt;
}

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject addGameButton, scrollViewContent, OpenGameButtonPrefab;
    [SerializeField]
    private string apiBaseUrl;
    [SerializeField]
    private string[] headers;

    private Coroutine updateGameListRoutine;

    private void Start()
    {
        addGameButton.GetComponent<Button>().onClick.AddListener(createGame);
        updateGameListRoutine = StartCoroutine(UpdateGameList());
    }

    private void createGame()
    {
        UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}OpenGames", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes($"{{ \"hostAddress\" : \"{GetLocalIPAddress()}\" }}");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        foreach (string header in headers)
        {
            string[] keyValuePair = header.Split(":");

            request.SetRequestHeader(keyValuePair[0], keyValuePair[1]);
        }

        request.SendWebRequest().completed += (AsyncOperation operation) =>
        {
            Debug.Log(request.downloadHandler.text);
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
            UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}OpenGames");

            foreach (string header in headers)
            {
                string[] keyValuePair = header.Split(":");

                request.SetRequestHeader(keyValuePair[0], keyValuePair[1]);
            }

            request.SendWebRequest().completed += (AsyncOperation operation) =>
            {
                string result = request.downloadHandler.text;
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
