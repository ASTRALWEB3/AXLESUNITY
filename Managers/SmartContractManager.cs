using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using UnityEngine.SceneManagement;

namespace Thirdweb.Unity.Examples
{
    // 2. CLASS MUST BE TOP-LEVEL (Removed "SmartContractIntegration" wrapper)
    public class SmartContractManager : MonoBehaviour
    {
        #region Inspector

        [Header("General Settings")]
        public bool AlwaysUpgradeToSmartWallet;
        public ulong ChainId; // This is your default chain (probably Base Sepolia per your other code)
        public GameObject LogPanel;

        [Header("Main Menu UI")]
        public Button OpenLoginPopupButton;

        [Header("Game Flow UI")]
        public Button PlayGameButton;            // New: Appears after login
        public TMP_InputField PlayerNameInput;
        public string GameSceneName = "WorldListScene";

        [Header("Wallet Stats UI")]
        public TMP_Text AddressText;             // New: Shows 0x123...
        public TMP_Text BalanceText;             // New: Shows 1.5 ETH

        [Header("Email Login Popup UI")]
        public GameObject EmailPopupPanel;
        public UnityEngine.UI.InputField EmailInputField;
        public Button SubmitEmailButton;
        public Button CloseEmailButton;

        [Header("World Shop Settings")]
        public TMP_InputField WorldNameInput;
        private String worldContractAddress = "0xf69A001Dbe2F06c442f1958feC2b99D98de3Adf0";

        #endregion

        #region Initialization

        private void Awake()
        {
            // Reset UI State
            if (LogPanel) LogPanel.SetActive(false);
            if (EmailPopupPanel) EmailPopupPanel.SetActive(false);
            if (WorldNameInput) WorldNameInput.gameObject.SetActive(false);

            // Hide Game/Stats UI initially
            if (PlayGameButton) PlayGameButton.gameObject.SetActive(false);
            if (AddressText) AddressText.gameObject.SetActive(false);
            if (BalanceText) BalanceText.gameObject.SetActive(false);
            if (PlayerNameInput) PlayerNameInput.gameObject.SetActive(false);

            // Ensure Login button is visible initially
            if (OpenLoginPopupButton) OpenLoginPopupButton.gameObject.SetActive(true);
            // --- LISTENERS --

            // 1. Login Button
            if (OpenLoginPopupButton)
            {
                OpenLoginPopupButton.onClick.RemoveAllListeners();
                OpenLoginPopupButton.onClick.AddListener(OpenEmailPopup);
            }

            // 2. Check Balance Button (NEW)
            if (PlayGameButton)
            {
                PlayGameButton.onClick.RemoveAllListeners();
                PlayGameButton.onClick.AddListener(OnPlayGameClicked);
            }

            // 3. Popup Buttons
            if (SubmitEmailButton)
            {
                SubmitEmailButton.onClick.RemoveAllListeners();
                SubmitEmailButton.onClick.AddListener(OnSubmitEmailClicked);
            }

            if (CloseEmailButton)
            {
                CloseEmailButton.onClick.RemoveAllListeners();
                CloseEmailButton.onClick.AddListener(() => EmailPopupPanel.SetActive(false));
            }
        }

        private void Start()
        {
            // Auto-detect if already connected (e.g. session persistence)
            if (ThirdwebManager.Instance.ActiveWallet != null)
            {
                UpdateWalletUI();
            }
        }

        private void LogPlayground(string message)
        {
            if (LogPanel)
            {
                LogPanel.GetComponentInChildren<TMP_Text>().text = message;
                LogPanel.SetActive(true);
            }
            Debug.Log(message);
        }

        #endregion

        #region Game Flow & UI Updates

        // Called automatically after successful login
        private async void UpdateWalletUI()
        {
            if (!WalletConnected()) return;

            try
            {
                var wallet = ThirdwebManager.Instance.ActiveWallet;
                var address = await wallet.GetAddress();

                // 1. Fetch Balance
                ulong liskSepoliaChainId = 4202; // Or use this.ChainId
                var balance = await wallet.GetBalance(liskSepoliaChainId);
                var chainData = await Utils.GetChainMetadata(ThirdwebManager.Instance.Client, liskSepoliaChainId);

                string readableBalance = Utils.ToEth(balance.ToString(), 4, true);
                string symbol = chainData.NativeCurrency.Symbol;
                string shortAddress = "";
                // 2. Update UI Texts
                if (AddressText)
                {
                    AddressText.gameObject.SetActive(true);
                    // Shorten address: 0x1234...5678
                    shortAddress = address.Substring(0, 6) + "..." + address.Substring(address.Length - 4);
                    AddressText.text = shortAddress;
                }

                if (BalanceText)
                {
                    BalanceText.gameObject.SetActive(true);
                    BalanceText.text = $"{readableBalance} {symbol}";
                }

                // 3. Swap Buttons (Hide Login, Show Play)
                if (OpenLoginPopupButton) OpenLoginPopupButton.gameObject.SetActive(false);
                if (PlayGameButton) PlayGameButton.gameObject.SetActive(true);

                if (PlayerNameInput)
                {
                    PlayerNameInput.gameObject.SetActive(true);
                    // Pre-fill with saved name
                    PlayerNameInput.text = PlayerPrefs.GetString("PlayerName", "");
                }

                // 4. Close popup if open
                if (EmailPopupPanel) EmailPopupPanel.SetActive(false);

                LogPanel.SetActive(false);

                // LogPlayground($"Ready! {shortAddress}");
            }
            catch (Exception e)
            {
                // LogPlayground($"Error updating UI: {e.Message}");
                Debug.LogError($"Error updating wallet UI: {e.Message}");
            }
        }

        private void OnPlayGameClicked()
        {
            if (PlayerNameInput)
            {
                PlayerNameInput.gameObject.SetActive(true);
                // Pre-fill with saved name
                // PlayerNameInput.text = PlayerPrefs.GetString("PlayerName", "");
                string name = PlayerNameInput.text.Trim();
                PlayerPrefs.SetString("PlayerName", name);
                PlayerPrefs.Save(); //
            }

            // LogPlayground("Starting Game...");
            SceneController.Instance.LoadScene(GameSceneName);
        }

        #endregion

        #region Balance Logic (NEW)

        // private async void OnCheckBalanceClicked()
        // {
        //     if (!WalletConnected())
        //     {
        //         LogPlayground("Please Connect Address first.");
        //         return;
        //     }

        //     LogPlayground("Fetching Balance on Lisk Sepolia (4202)...");

        //     try
        //     {
        //         // Explicitly defining Lisk Sepolia Chain ID
        //         ulong liskSepoliaChainId = 4202;

        //         // Get balance specific to this chain ID
        //         var balance = await ThirdwebManager.Instance.ActiveWallet.GetBalance(liskSepoliaChainId);

        //         // Get chain metadata to know the currency symbol (ETH/LSK/etc)
        //         var chainData = await Utils.GetChainMetadata(ThirdwebManager.Instance.Client, liskSepoliaChainId);

        //         // Format the result (e.g. 1.5 ETH)
        //         // Utils.ToEth takes the big integer and divides by 10^18 to give you a readable number
        //         string readableBalance = Utils.ToEth(balance.ToString(), 4, true); 
        //         string symbol = chainData.NativeCurrency.Symbol;

        //         LogPlayground($"Balance: {readableBalance} {symbol}");
        //     }
        //     catch (Exception e)
        //     {
        //         LogPlayground($"Error fetching balance: {e.Message}");
        //     }
        // }

        #endregion

        #region Email Wallet Logic

        public void OpenEmailPopup()
        {
            if (EmailPopupPanel != null)
            {
                EmailPopupPanel.SetActive(true);
                if (EmailInputField) EmailInputField.text = "";
                // LogPlayground("Please enter your email.");
            }
            else
            {
                Debug.LogError("EmailPopupPanel is not assigned in the Inspector!");
            }
        }

        private async void OnSubmitEmailClicked()
        {
            string emailAddress = EmailInputField.text;

            if (string.IsNullOrEmpty(emailAddress))
            {
                LogPlayground("Please enter a valid email address.");
                return;
            }
            EmailPopupPanel.SetActive(false);

            // Don't close panel immediately, show loading state text
            LogPlayground($"Connecting with {emailAddress}...\nPlease check your inbox for the verification code.");

            try
            {
                var walletOptions = new WalletOptions(
                    provider: WalletProvider.InAppWallet,
                    chainId: this.ChainId,
                    new InAppWalletOptions(email: emailAddress)
                );

                var wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);

                if (this.AlwaysUpgradeToSmartWallet)
                {
                    wallet = await ThirdwebManager.Instance.UpgradeToSmartWallet(
                        wallet,
                        chainId: this.ChainId,
                        smartWalletOptions: new SmartWalletOptions(sponsorGas: true)
                    );
                }

                // Login Successful! Update UI
                UpdateWalletUI();
            }
            catch (Exception e)
            {
                this.LogPlayground($"Login Failed: {e.Message}");
                // Ensure popup stays open to retry
                if (EmailPopupPanel) EmailPopupPanel.SetActive(true);
            }
        }

        private bool WalletConnected()
        {
            return ThirdwebManager.Instance.ActiveWallet != null;
        }

        #endregion

        #region World Shop Logic

        // public void OnClick_BuyWorld()
        // {
        //     if (WorldNameInput == null) return;
        //     if (!WorldNameInput.gameObject.activeSelf)
        //     {
        //         WorldNameInput.gameObject.SetActive(true);
        //         LogPlayground("Enter world name and click Buy again.");
        //         return;
        //     }
        //     string worldName = WorldNameInput.text;
        //     WorldNameInput.gameObject.SetActive(false);
        //     Contract_Flow_BuyWorld(worldName);
        // }

        // private async void Contract_Flow_BuyWorld(string worldToBuy)
        // {
        //     if (!WalletConnected())
        //     {
        //         LogPlayground("Please Login first.");
        //         return;
        //     }
        //     try
        //     {
        //         LogPlayground("Step 1: Approving token spend...");
        //         var shopContract = await ThirdwebManager.Instance.GetContract(worldContractAddress, this.ChainId);
        //         string tokenAddress = await shopContract.Read<string>("gameToken");
        //         BigInteger worldPrice = await shopContract.Read<BigInteger>("worldPrice");
        //         if (worldPrice > 0)
        //         {
        //             var tokenContract = await ThirdwebManager.Instance.GetContract(tokenAddress, this.ChainId);
        //             await tokenContract.ERC20_Approve(
        //                 wallet: ThirdwebManager.Instance.ActiveWallet,
        //                 spenderAddress: worldContractAddress,
        //                 amount: worldPrice
        //             );
        //         }
        //         LogPlayground("Step 2: Buying World...");
        //         var receipt = await shopContract.Write(
        //             ThirdwebManager.Instance.ActiveWallet,
        //             "buyWorld",
        //             0,
        //             worldToBuy
        //         );
        //         LogPlayground($"Success! Purchased {worldToBuy}.\nTx: {receipt.transactionHash}");
        //     }
        //     catch (Exception e)
        //     {
        //         LogPlayground($"Error: {e.Message}");
        //     }
        // }

        #endregion
    }
}