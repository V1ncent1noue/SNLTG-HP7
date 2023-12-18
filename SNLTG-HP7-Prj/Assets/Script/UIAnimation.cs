using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 0.2f;

    private Image image;
    private int currentFrameIndex;
    private float timer;

    private void Start()
    {
        image = GetComponent<Image>();
        currentFrameIndex = 0;
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
            image.sprite = frames[currentFrameIndex];
        }
    }
}
