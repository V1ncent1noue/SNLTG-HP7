using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Color targetColor;

    private Color initialColor;

    private void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();

        initialColor = textMeshPro.color;

        StartCoroutine(ColorChangeCoroutine());
    }

    private IEnumerator ColorChangeCoroutine()
    {
        while (true)
        {
            textMeshPro.color = targetColor;

            // Wait for 0.5 seconds
            yield return new WaitForSeconds(0.5f);

            textMeshPro.color = initialColor;

            yield return new WaitForSeconds(0.5f);
        }
    }

}
