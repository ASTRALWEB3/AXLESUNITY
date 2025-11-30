using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextWriter : MonoBehaviour
{
    private static TextWriter instance;
    private List<TextWriterSingle> textWriterSingleList;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
        {
            GameObject textWriterGameObject = new GameObject("TextWriter");
            instance = textWriterGameObject.AddComponent<TextWriter>();
            DontDestroyOnLoad(textWriterGameObject);
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        textWriterSingleList = new List<TextWriterSingle>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (textWriterSingleList == null) return;

        for (int i = 0; i < textWriterSingleList.Count; i++)
        {
            bool destroyInstance = textWriterSingleList[i].Update();
            if (destroyInstance)
            {
                textWriterSingleList.RemoveAt(i);
                i--;
            }
        }
    }

    public static TextWriterSingle AddWriter_Static(TextMeshPro uiText, string textToWrite, float timePerCharacter, bool invisibleCharacters, bool removeWriterBeforeAdd, Action onComplete)
    {
        Initialize(); // Ensure instance exists

        if (instance == null)
        {
            Debug.LogError("TextWriter instance is null!");
            return null;
        }

        if (removeWriterBeforeAdd)
        {
            instance.RemoveWriter(uiText);
        }

        return instance.AddWriter(uiText, textToWrite, timePerCharacter, invisibleCharacters, onComplete);
    }

    private TextWriterSingle AddWriter(TextMeshPro uiText, string textToWrite, float timePerCharacter, bool invisibleCharacters, Action onComplete)
    {
        TextWriterSingle textWriterSingle = new TextWriterSingle(uiText, textToWrite, timePerCharacter, invisibleCharacters, onComplete);
        textWriterSingleList.Add(textWriterSingle);
        return textWriterSingle;
    }

    private void RemoveWriter(TextMeshPro uiText)
    {
        for (int i = 0; i < textWriterSingleList.Count; i++)
        {
            if (textWriterSingleList[i].GetUIText() == uiText)
            {
                textWriterSingleList.RemoveAt(i);
                i--;
            }
        }
    }

    public class TextWriterSingle
    {
        private TextMeshPro textMeshPro;
        private string textToWrite;
        private int characterIndex;
        private float timePerCharacter;
        private float timer;
        private bool invisibleCharacters;
        private Action onComplete;

        public TextWriterSingle(TextMeshPro textMeshPro, string textToWrite, float timePerCharacter, bool invisibleCharacters, Action onComplete)
        {
            this.textMeshPro = textMeshPro;
            this.textToWrite = textToWrite;
            this.timePerCharacter = timePerCharacter;
            this.invisibleCharacters = invisibleCharacters;
            this.onComplete = onComplete;
            characterIndex = 0;
            timer = 0f;
        }

        public bool Update()
        {
            if (textMeshPro == null) return true;

            timer -= Time.deltaTime;
            
            while (timer <= 0f)
            {
                timer += timePerCharacter;
                characterIndex++;

                string text = textToWrite.Substring(0, characterIndex);

                if (invisibleCharacters)
                {
                    text += "<color=#00000000>" + textToWrite.Substring(characterIndex) + "</color>";
                }

                textMeshPro.text = text;

                if (characterIndex >= textToWrite.Length)
                {
                    onComplete?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public TextMeshPro GetUIText()
        {
            return textMeshPro;
        }

        public bool IsActive()
        {
            return characterIndex < textToWrite.Length;
        }

        public void WriteAllAndDestroy()
        {
            textMeshPro.text = textToWrite;
            characterIndex = textToWrite.Length;
            onComplete?.Invoke();
        }
    }
}