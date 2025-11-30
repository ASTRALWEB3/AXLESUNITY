using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;

public class WorldListManager : MonoBehaviour
{
    [Header("Configuration")]
    public string graphqlEndpoint = "";
    public GameObject buttonPrefab;
    public Transform contentContainer;

    public int sampleSceneIndex = 2;


    // TESTING ONLY WHEN NO API WORLDS EXIST

    void Start()
    {
        // CHANGE THIS ALSO IF ALREADY PRODUCTION 
        if (useTestData)
        {
            StartCoroutine(FetchWorldsMock());
        }
        else
        {
            StartCoroutine(FetchWorlds());
        }
    }

    #region CHANGES
    public bool useTestData = true;
    IEnumerator FetchWorldsMock()
    {
        Debug.Log("Using Mock Data (No Server Connection)...");
        yield return new WaitForSeconds(0.5f); // Simulate network delay

        // Create fake data structure
        GraphQLResponse mockResponse = new GraphQLResponse();
        mockResponse.data = new DataContainer();
        mockResponse.data.worlds = new WorldList();
        mockResponse.data.worlds.items = new System.Collections.Generic.List<ApiWorldItem>();

        // Add Fake Item 1
        ApiWorldItem w1 = new ApiWorldItem { name = "Test World A", owner = "Dev", id = "101" };
        mockResponse.data.worlds.items.Add(w1);

        // Add Fake Item 2
        ApiWorldItem w2 = new ApiWorldItem { name = "Mega City", owner = "PlayerOne", id = "102" };
        mockResponse.data.worlds.items.Add(w2);

        // Process it as if it came from the server
        string json = JsonUtility.ToJson(mockResponse); // Or just pass the object directly if you refactor ProcessResponse

        // For simplicity, we just call the logic directly
        GenerateButtonsFromData(mockResponse);
    }

    [System.Serializable]
    public class GraphQLResponse
    {
        public DataContainer data;
    }

    [System.Serializable]
    public class DataContainer
    {
        public WorldList worlds;
    }

    [System.Serializable]
    public class WorldList
    {
        public System.Collections.Generic.List<ApiWorldItem> items;
    }

    [System.Serializable]
    public class ApiWorldItem
    {
        public string id;
        public string name;
        public string owner;
    }

    IEnumerator FetchWorlds()
    {
        string query = "{ worlds { items { id name owner } } }";
        string jsonPayload = "{\"query\": \"" + query + "\"}";

        UnityWebRequest request = new UnityWebRequest(graphqlEndpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Contacting GraphQL...");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            // Fallback to mock on error?
            // StartCoroutine(FetchWorldsMock()); 
        }
        else
        {
            Debug.Log("Data received!");
            ProcessResponse(request.downloadHandler.text);
        }
    }

    void ProcessResponse(string json)
    {
        GraphQLResponse result = JsonUtility.FromJson<GraphQLResponse>(json);
        GenerateButtonsFromData(result);
    }

    // Refactored generation logic to be reusable
    void GenerateButtonsFromData(GraphQLResponse result)
    {
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        // Always add the Farm World
        ApiWorldItem farmWorld = new ApiWorldItem();
        farmWorld.name = "FARM";
        farmWorld.owner = "System";
        farmWorld.id = "FARM_ID";
        GenerateButton(farmWorld);

        // Add fetched/mocked worlds
        if (result.data != null && result.data.worlds != null)
        {
            foreach (ApiWorldItem world in result.data.worlds.items)
            {
                GenerateButton(world);
            }
        }
    }

    void GenerateButton(ApiWorldItem world)
    {
        GameObject newBtn = Instantiate(buttonPrefab, contentContainer);

        TMP_Text btnText = newBtn.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
        {
            btnText.text = world.name;
        }

        Button btnComp = newBtn.GetComponent<Button>();

        if (world.name == "FARM")
        {
            btnComp.onClick.AddListener(async () =>
            {
                Debug.Log("Connecting to Farm World...");
                    await NetworkManager.Instance.StartGame(Fusion.GameMode.Client, sampleSceneIndex);
                // if (NetworkManager.Instance != null)
                // {
                // }
                // else
                // {
                //     Debug.LogError("NetworkManager not found!");
                // }
            });
        }
        else
        {
            btnComp.onClick.AddListener(() =>
            {
                Debug.Log("Selected World: " + world.name);
                // Logic for other worlds can go here
                // e.g. Connect to a specific room name based on world.id
            });
        }
    }
    #endregion

    #region DONT CHANGE THIS
    // IEnumerator FetchWorlds()
    // {
    //     string query = "{ worlds { items { id name owner } } }";

    //     string jsonPayload = "{\"query\": \"" + query + "\"}";

    //     UnityWebRequest request = new UnityWebRequest(graphqlEndpoint, "POST");
    //     byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
    //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //     request.downloadHandler = new DownloadHandlerBuffer();
    //     request.SetRequestHeader("Content-Type", "application/json");

    //     Debug.Log("Contacting GraphQL...");
    //     yield return request.SendWebRequest();

    //     if (request.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.LogError("Error: " + request.error);
    //     }
    //     else
    //     {
    //         Debug.Log("Data received!");
    //         ProcessResponse(request.downloadHandler.text);
    //     }
    // }

    // void ProcessResponse(string json)
    // {
    //     GraphQLResponse result = JsonUtility.FromJson<GraphQLResponse>(json);

    //     foreach (Transform child in contentContainer) Destroy(child.gameObject);

    //     ApiWorldItem farmWorld = new ApiWorldItem();
    //     farmWorld.name = "FARM";
    //     farmWorld.owner = "System";
    //     farmWorld.id = "FARM_ID";

    //     GenerateButton(farmWorld);

    //     if (result.data != null && result.data.worlds != null)
    //     {
    //         foreach (ApiWorldItem world in result.data.worlds.items)
    //         {
    //             GenerateButton(world);
    //         }
    //     }
    // }

    // void GenerateButton(ApiWorldItem world)
    // {
    //     GameObject newBtn = Instantiate(buttonPrefab, contentContainer);

    //     TMP_Text btnText = newBtn.GetComponentInChildren<TMP_Text>();
    //     if (btnText != null)
    //     {
    //         btnText.text = world.name;
    //     }

    //     Button btnComp = newBtn.GetComponent<Button>();

    //     if (world.name == "FARM")
    //     {
    //         btnComp.onClick.AddListener(async () =>
    //         {
    //             Debug.Log("Connecting to Farm World...");

    //             // --- FIXED LOGIC ---
    //             // Don't load scene directly. Ask NetworkManager to Connect.
    //             // Fusion will automatically load the scene that the Server is on.
    //             if (NetworkManager.Instance != null)
    //             {
    //                 // We pass sampleSceneIndex, but technically Client loads whatever scene the Server is on.
    //                 await NetworkManager.Instance.StartGame(Fusion.GameMode.Client, sampleSceneIndex);
    //             }
    //             else
    //             {
    //                 Debug.LogError("NetworkManager not found! Is it in the Main Menu?");
    //             }
    //         });
    //     }
    //     else
    //     {
    //         // TO DO : change to the world scene
    //         btnComp.onClick.AddListener(() =>
    //         {
    //             Debug.Log("Selected API World: " + world.name);
    //         });
    //     }
    // }
    #endregion
}