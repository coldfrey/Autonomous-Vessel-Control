using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPlotter : MonoBehaviour
{
    public RectTransform plotArea; // Drag your UI Image here in the inspector
    public Color plotLineColor = Color.green;
    private List<float> rewards = new List<float>();

    // Call this method to add a new reward value
    public void AddReward(float reward)
    {
        rewards.Add(reward);
        DrawPlot();
    }

    private void DrawPlot()
    {
        // Clear previous lines
        foreach (Transform child in plotArea)
        {
            Destroy(child.gameObject);
        }

        if (rewards.Count < 2) return; // Need at least 2 points to draw a line

        for (int i = 1; i < rewards.Count; i++)
        {
            DrawLine(rewards[i - 1], rewards[i], i - 1, i);
        }
    }

    private void DrawLine(float y1, float y2, int x1, int x2)
    {
        GameObject line = new GameObject("Line", typeof(Image));
        line.transform.SetParent(plotArea, false);
        line.GetComponent<Image>().color = plotLineColor;

        RectTransform rt = line.GetComponent<RectTransform>();
        Vector2 position = new Vector2(x1, y1);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2)), 2); // 2 is the thickness of the line
        rt.pivot = new Vector2(0, 0.5f);
        float angle = Mathf.Atan2(y2 - y1, x2 - x1) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
