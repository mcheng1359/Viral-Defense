using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Renderer[] renderers;
    public Color startingColor;
    public Color interpolateColor;
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float step = Mathf.Sin(Time.time);
        foreach(Renderer renderer in renderers){
            renderer.material.color = Color.Lerp(startingColor, interpolateColor, step);
        }
    }
}
