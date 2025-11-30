using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
// using Thirdweb; // Commented out to prevent errors
using System.Threading.Tasks;

// --- JSON Data Classes ---
[System.Serializable]
public class TransferItem
{
    public string id;
    public string from;
    public string to;
    public string tokenId;
}

[System.Serializable]
public class NftTransfers
{
    public List<TransferItem> items;
}

[System.Serializable]
public class Data
{
    public NftTransfers nftTransfers;
}

[System.Serializable]
public class DragonGraphQLResponse
{
    public Data data;
}

// --- Main Manager Class ---
public class DragonUnlockManager : MonoBehaviour
{
    [Header("Configuration")]
    public bool useMockData = true; // <-- NEW: Check this to skip server validation
    public string graphQLEndpoint = "https://c2e3e2367b377b.lhr.life/";

    [Header("Testing")]
    public string testWalletAddress = "0x2fc07cf2fd06e52d3acc187cd75e5e7380c97af4";

    [Header("UI References")]
    public Button dragonUnlockButton;

    [Header("Cosmetic Settings")]
    public AnimatorOverrideController dragonSkinController;

    private string currentWalletAddress;

    void Start()
    {
        if (dragonUnlockButton != null)
            dragonUnlockButton.gameObject.SetActive(false);

        // --- TEST MODE ---
        currentWalletAddress = testWalletAddress;
        Debug.Log("Using Test Wallet Address: " + currentWalletAddress);

        if (useMockData)
        {
            StartCoroutine(CheckNftOwnershipMock());
        }
        else
        {
            StartCoroutine(CheckNftOwnership());
        }
    }

    // --- MOCK DATA CHECK (No Server Required) ---
    IEnumerator CheckNftOwnershipMock()
    {
        Debug.Log("Using Mock Data... Simulating Server Response...");
        yield return new WaitForSeconds(1.0f); // Fake loading delay

        // Create a fake JSON response that guarantees success
        // We match the 'to' address with our current wallet so the logic passes
        string mockJson = $@"
        {{
            ""data"": {{
                ""nftTransfers"": {{
                    ""items"": [
                        {{
                            ""id"": ""mock_transfer_1"",
                            ""from"": ""0x0000000000000000000000000000000000000000"",
                            ""to"": ""{currentWalletAddress}"",
                            ""tokenId"": ""0""
                        }}
                    ]
                }}
            }}
        }}";

        Debug.Log("Mock Data Received.");
        ProcessResponse(mockJson);
    }

    IEnumerator CheckNftOwnership()
    {
        // Query to check for tokenId "0"
        string query = "{\"query\": \"{ nftTransfers(limit: 10, orderBy: \\\"timestamp\\\", orderDirection: \\\"desc\\\") { items { id from to tokenId blockNumber timestamp transactionHash } totalCount pageInfo { hasNextPage endCursor } } }\"}";

        UnityWebRequest request = new UnityWebRequest(graphQLEndpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(query);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("GraphQL Error: " + request.error);
        }
        else
        {
            ProcessResponse(request.downloadHandler.text);
        }
    }

    void ProcessResponse(string jsonResult)
    {
        DragonGraphQLResponse response = JsonUtility.FromJson<DragonGraphQLResponse>(jsonResult);

        if (response != null && response.data != null && response.data.nftTransfers != null)
        {
            bool playerOwnsTokenZero = false;

            foreach (TransferItem item in response.data.nftTransfers.items)
            {
                if (item.tokenId == "0" && item.to.Equals(currentWalletAddress, System.StringComparison.OrdinalIgnoreCase))
                {
                    playerOwnsTokenZero = true;
                    break;
                }
            }

            if (playerOwnsTokenZero)
            {
                Debug.Log("Ownership Verified! Unlocking Cosmetic Button.");
                UnlockDragonButton();
            }
            else
            {
                Debug.Log("Ownership Verification Failed (Mock or Real). Token ID 0 not found on this wallet.");
            }
        }
    }

    void UnlockDragonButton()
    {
        if (dragonUnlockButton != null)
        {
            dragonUnlockButton.gameObject.SetActive(true);
            dragonUnlockButton.onClick.RemoveAllListeners();
            dragonUnlockButton.onClick.AddListener(OnApplyCosmeticClicked);
        }
    }

    public void OnApplyCosmeticClicked()
    {
        Debug.Log("Searching for Active Player...");

        // 1. Find the object tagged "Player" in the scene right now
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null && dragonSkinController != null)
        {
            Debug.Log("Found Player: " + foundPlayer.name);

            // 2. Get the Animator
            Animator playerAnim = foundPlayer.GetComponent<Animator>();

            if (playerAnim != null)
            {
                // 3. Swap the Controller
                playerAnim.runtimeAnimatorController = dragonSkinController;
                Debug.Log("Skin APPLIED to " + foundPlayer.name);

                // 4. Force a refresh (Helps if the animation gets stuck)
                playerAnim.Rebind();
                playerAnim.Update(0f);

                dragonUnlockButton.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("Found player object, but it has no Animator!");
            }
        }
        else
        {
            Debug.LogError("Could not find any object with tag 'Player' in the scene! Make sure your Player Prefab is Tagged!");
        }
    }
}