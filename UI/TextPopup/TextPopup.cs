using UnityEngine;
using TMPro;

public class TextPopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(string text, Vector3 position)
    {
        textMesh.text = text;
        transform.position = position;
        textColor = textMesh.color;
        disappearTimer = 1f;
        moveVector = new Vector3(0, 20f);
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        disappearTimer -= Time.deltaTime;

        if (disappearTimer > 0.5f)
        {
            // First half: stay solid
            textColor.a = 1f;
        }
        else
        {
            // Second half: fade out
            textColor.a = disappearTimer / 0.5f;
        }

        textMesh.color = textColor;

        if (disappearTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}