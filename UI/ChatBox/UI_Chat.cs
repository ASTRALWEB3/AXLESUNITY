using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UI_Chat : MonoBehaviour
{

    private void Start()
    {
        Button chatButton = GetComponent<Button>();

        if (chatButton != null)
        {
            chatButton.onClick.AddListener(() =>
            {
                UI_Blocker.Show_Static();
                UI_InputChat.Show_Static("Chat Window", "", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ", 20,
                    () =>
                    {
                        // Cancel callback
                        // CMDebug.TextPopupMouse("Cancel!");
                        UI_Blocker.Hide_Static();
                    },
                    (string inputText) =>
                    {
                        // OK callback
                        // CMDebug.TextPopupMouse("Ok: " + inputText);
                        UI_Blocker.Hide_Static();
                    });
            });
        }
        else
        {
            Debug.LogError("Button or inputWindow is not set. Check Inspector.", this);
        }
    }
}
