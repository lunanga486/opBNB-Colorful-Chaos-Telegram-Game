using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using Thirdweb.Unity;
using TMPro;
using UnityEngine.UI;
using System.Numerics;
using System;
using System.Data;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Collections.LowLevel.Unsafe;

public class ConnectWalletManager : MonoBehaviour
{
    private const string PlayerTokenKey = "PlayerToken";
    public TMP_Text textLog;
    public TMP_Text tokenBalanceText;

    public string Address { get; private set; }
    public static BigInteger ChainId = 204;

    public UnityEngine.UI.Button playButton;
    public UnityEngine.UI.Button claimTokenButton;
    public UnityEngine.UI.Button getBalanceButton;

    string customSmartContractAddress = "0x41a5124180f090bd3A320D8e049Dd5ecc091025d";
    string abiString = "[{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"totalPoints\",\"type\":\"uint256\"}],\"name\":\"ChaosPointsUpdated\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"getChaosPoints\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"incrementChaosPoints\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"player\",\"type\":\"address\"}],\"name\":\"resetChaosPoints\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";

    int tokenAmount = 1;

    string notEnoughToken = " BNB";

    private void Start()
    {
        // Get the current value of PlayerToken, defaulting to 0 if it doesn't exist
        int currentTokens = PlayerPrefs.GetInt(PlayerTokenKey, 0);
        tokenBalanceText.text = "Token Owned: " + currentTokens.ToString();
    }

    // Method to switch to the Gameplay scene
    public void SwitchToMainMenuScene()
    {
        // Check if the PlayerTokenKey exists in PlayerPrefs
        if (PlayerPrefs.HasKey(PlayerTokenKey))
        {
            int currentTokens = PlayerPrefs.GetInt(PlayerTokenKey);

            if (currentTokens > 0)
            {
                // Subtract 1 from PlayerToken
                currentTokens--;
                PlayerPrefs.SetInt(PlayerTokenKey, currentTokens);
                PlayerPrefs.Save();
                currentTokens = PlayerPrefs.GetInt(PlayerTokenKey);
                tokenBalanceText.text = "Token Owned: " + currentTokens.ToString();

                Debug.Log("PlayerToken reduced by 1. Remaining: " + currentTokens);
                // Switch to the MainMenu scene
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.Log("PlayerToken value is 0. Cannot deduct further.");
                textLog.text = "Get more tokens to play";
            }
        }
        else
        {
            // Debug message if PlayerTokenKey doesn't exist
            Debug.LogWarning("PlayerTokenKey does not exist in PlayerPrefs!");
            textLog.text = "Get 1 token to play";
        }
    }

    public void AddPlayerToken()
    {
        // Get the current value of PlayerToken, defaulting to 0 if it doesn't exist
        int currentTokens = PlayerPrefs.GetInt(PlayerTokenKey, 0);

        // Increment the value by 1
        currentTokens++;

        // Save the updated value back to PlayerPrefs
        PlayerPrefs.SetInt(PlayerTokenKey, currentTokens);

        // Ensure PlayerPrefs is saved immediately
        PlayerPrefs.Save();
        currentTokens = PlayerPrefs.GetInt(PlayerTokenKey, 0);
        tokenBalanceText.text = "Token Owned: " + currentTokens.ToString();

        Debug.Log("PlayerToken updated to: " + currentTokens);
    }

    private void HideAllButtons()
    {
        playButton.interactable = false;
        claimTokenButton.interactable = false;
        getBalanceButton.interactable = false;
    }

    private void ShowAllButtons()
    {
        playButton.interactable = true;
        claimTokenButton.interactable = true;
        getBalanceButton.interactable = true;
    }

    private void UpdateStatus(string messageShow)
    {
        textLog.text = messageShow;
    }

    private void BoughtSuccessFully()
    {
        AddPlayerToken();
        UpdateStatus("Got 1 Tokens");
    }
    IEnumerator WaitAndExecute()
    {
        Debug.Log("Coroutine started, waiting for 3 seconds...");
        yield return new WaitForSeconds(3f); // Chờ 3 giây
        Debug.Log("3 seconds have passed!");
        BoughtSuccessFully();
        ShowAllButtons();
    }

    public async void Claim1Token()
    {
        var wallet = ThirdwebManager.Instance.GetActiveWallet();
        var contract = await ThirdwebManager.Instance.GetContract(
           customSmartContractAddress,
           ChainId,
           abiString
       );
        var address = await wallet.GetAddress();

        // Gọi hàm `submitScore` trong hợp đồng với điểm số (score)
        await ThirdwebContract.Write(wallet, contract, "incrementChaosPoints", 0, address);

        var result = ThirdwebContract.Read<int>(contract, "getChaosPoints", address);
        Debug.Log("result: " + result);
    }

    public async void GetTokens()
    {
        HideAllButtons();
        UpdateStatus("Getting 1 Token...");
        var wallet = ThirdwebManager.Instance.GetActiveWallet();
        var balance = await wallet.GetBalance(chainId: ChainId);
        var balanceEth = Utils.ToEth(wei: balance.ToString(), decimalsToDisplay: 4, addCommas: true);
        Debug.Log("balanceEth1: " + balanceEth);
        if (float.Parse(balanceEth) <= 0f)
        {
            UpdateStatus("Not Enough" + notEnoughToken);
            ShowAllButtons();
            return;
        }
        StartCoroutine(WaitAndExecute());
        try
        {
            Claim1Token();
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred during the transfer: {ex.Message}");
        }
    }
}