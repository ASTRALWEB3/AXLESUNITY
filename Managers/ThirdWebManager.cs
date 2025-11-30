using System;
using System.Numerics;
using Thirdweb;
using Thirdweb.Unity;
using TMPro;
using UnityEngine;

public class ThirdWebManager : MonoBehaviour
{
    public ulong ChainId;
    public string Email;
    public string Phone;
    public Transform ActionButtonParent;
    public GameObject ActionButtonPrefab;
    public GameObject LogPanel;

    private String walletAddress = "0x854083e5cdDa8Bed2AfB5C1bF2C03A33894f1cF8";
    private String worldContractAddress = "0xf69A001Dbe2F06c442f1958feC2b99D98de3Adf0";

    private void LogPlayground(string message)
    {
        this.LogPanel.GetComponentInChildren<TMP_Text>().text = message;
        // ThirdwebDebug.Log(message);
        this.LogPanel.SetActive(true);
    }

    private bool WalletConnected()
    {
        var isConnected = ThirdwebManager.Instance.ActiveWallet != null;
        if (!isConnected)
        {
            this.LogPlayground("Please authenticate to connect a wallet first.");
        }
        return isConnected;
    }

    private async void Wallet_Email()
    {
        if (string.IsNullOrEmpty(this.Email))
        {
            this.LogPlayground("Please enter a valid email address in the scene's PlaygroundManager.");
            return;
        }

        var walletOptions = new WalletOptions(provider: WalletProvider.InAppWallet, chainId: this.ChainId, new InAppWalletOptions(email: this.Email));
        var wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);
        var address = await wallet.GetAddress();
        this.LogPlayground($"[Email] Connected to wallet:\n{address}");
    }

    private async void ReadMyContractBalance()
    {
        // 1. Your contract address
        var myContractAddress = "0xE841Af22921c0B1fc6df8D028f8387ECe633FF58";

        // 2. Base Sepolia Chain ID
        var baseSepoliaChainId = 4202;

        // 3. The address you want to check the balance of
        // !!! YOU MUST CHANGE THIS to the wallet address you want to query !!!
        var addressToQuery = "0x854083e5cdDa8Bed2AfB5C1bF2C03A33894f1cF8";

        // 4. Get the contract instance
        var contract = await ThirdwebManager.Instance.GetContract(
            address: myContractAddress,
            chainId: baseSepoliaChainId
        );

        // 5. Define the method signature
        var method = "function balanceOf(address account) view returns (uint256)";

        // 6. Call the Read method, passing the 'addressToQuery' as a parameter
        var balance = await contract.Read<BigInteger>(method, new object[] { addressToQuery });

        // 7. Log the result
        this.LogPlayground($"Balance of {addressToQuery}:\n{balance.ToString()}");
    }

    private async void Contract_Read()
    {
        var usdcContractAddressArbitrum = "0xE841Af22921c0B1fc6df8D028f8387ECe633FF58";
        var contract = await ThirdwebManager.Instance.GetContract(address: usdcContractAddressArbitrum, chainId: ChainId);
        var result = await contract.ERC20_BalanceOf(ownerAddress: await Utils.GetAddressFromENS(ThirdwebManager.Instance.Client, "vitalik.eth"));
        var tokenSymbol = await contract.ERC20_Symbol();
        var resultFormatted = Utils.ToEth(result.ToString(), 6, true) + " " + tokenSymbol;
        this.LogPlayground($"ThirdwebContract.ERC20_BalanceOf Result:\n{resultFormatted}");
    }
    public async void Contract_Flow_BuyWorld()
    {
        if (!this.WalletConnected())
        {
            this.LogPlayground("Wallet not connected.");
            return;
        }

        string worldToBuy = "my-awesome-world";

        try
        {
            // ---------------------------------
            // STEP 1: APPROVE TOKEN SPEND
            // ---------------------------------
            this.LogPlayground("Step 1: Approving token spend...");

            // Get the shop contract to read data from it
            var shopContract = await ThirdwebManager.Instance.GetContract(worldContractAddress, this.ChainId);

            // Read the 'gameToken' address and 'worldPrice' from the shop contract
            string tokenAddress = await shopContract.Read<string>("gameToken");
            BigInteger worldPrice = await shopContract.Read<BigInteger>("worldPrice");

            if (worldPrice == 0)
            {
                this.LogPlayground("World price is 0, cannot buy.");
                return;
            }

            this.LogPlayground($"Shop contract: {worldContractAddress}");
            this.LogPlayground($"Token contract: {tokenAddress}");
            this.LogPlayground($"World price: {worldPrice.ToString()}");

            // Get the TOKEN contract instance
            var tokenContract = await ThirdwebManager.Instance.GetContract(tokenAddress, this.ChainId);

            // Call ERC20_Approve on the TOKEN contract, approving the SHOP contract to spend our tokens
            var approveReceipt = await tokenContract.ERC20_Approve(
                wallet: ThirdwebManager.Instance.ActiveWallet,
                spenderAddress: worldContractAddress,
                amount: worldPrice
            );

            this.LogPlayground($"Approve Transaction Receipt:\n{approveReceipt}");

            // ---------------------------------
            // STEP 2: BUY THE WORLD
            // ---------------------------------
            this.LogPlayground("Step 2: Buying the world...");

            // Call 'buyWorld' on the SHOP contract.
            // It will now pull the tokens we just approved.
            var buyReceipt = await shopContract.Write(
                 ThirdwebManager.Instance.ActiveWallet, // ADDED: Pass the active wallet
                 "buyWorld",
                    0,
                 worldToBuy
             );

            this.LogPlayground($"BuyWorld Transaction Receipt:\n{buyReceipt}");
            this.LogPlayground($"Successfully purchased '{worldToBuy}'!");
        }
        catch (Exception e)
        {
            this.LogPlayground($"Error during buyWorld flow: {e.Message}");
        }
    }

    public async void Contract_Read_GetMyWorlds()
    {
        if (!this.WalletConnected())
        {
            this.LogPlayground("Wallet not connected.");
            return;
        }

        try
        {
            // 1. Get the connected wallet's address
            string myAddress = await ThirdwebManager.Instance.ActiveWallet.GetAddress();
            this.LogPlayground($"Checking for worlds owned by: {myAddress}...");

            // 2. Get the shop contract instance
            var shopContract = await ThirdwebManager.Instance.GetContract(worldContractAddress, this.ChainId);

            // 3. Call the 'Read' method. The return type is string[]
            string[] myWorlds = await shopContract.Read<string[]>(
                "getWorldsByOwner",
                myAddress
            );

            // 4. Log the results
            if (myWorlds == null || myWorlds.Length == 0)
            {
                this.LogPlayground("You do not own any worlds.");
                return;
            }

            this.LogPlayground($"Found {myWorlds.Length} world(s):");
            foreach (string world in myWorlds)
            {
                this.LogPlayground($"- {world}");
            }
        }
        catch (Exception e)
        {
            this.LogPlayground($"Error calling getWorldsByOwner: {e.Message}");
        }
    }

}