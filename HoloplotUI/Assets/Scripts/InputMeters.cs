using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class InputMeters : MonoBehaviour
{
    public OSCReceiver OSCReceiver;
    public int NumberOfChannels = 8; // User-defined number of channels, editable in the Inspector

    private List<string> channelLevelIn = new List<string>();
    private List<Slider> channelSliders = new List<Slider>();

    // Start is called before the first frame update
    void Start()
    {
        InitializeChannelsAndSliders();
    }

    private void InitializeChannelsAndSliders()
    {
        for (int i = 1; i <= NumberOfChannels; i++)
        {
            // Set channel string paths
            channelLevelIn.Add($"/channelIn/{i}");

            // Find corresponding sliders
            Slider channelSlider = GameObject.Find($"InCh{i}").GetComponent<Slider>();
            if (channelSlider != null)
            {
                channelSliders.Add(channelSlider);
            }
            else
            {
                Debug.LogWarning($"Slider for InCh{i} not found. Make sure the GameObject is named correctly.");
            }

            // Bind OSC messages to corresponding handlers
            int index = i - 1; // Closure issue prevention
            OSCReceiver.Bind(channelLevelIn[index], message => ReceivedInput(index, message));
        }
    }

    // Generalized input handler
    private void ReceivedInput(int index, OSCMessage message)
    {
        if (message.ToFloat(out var value))
        {
            if (index >= 0 && index < channelSliders.Count)
            {
                channelSliders[index].value = value;
            }
        }
    }
}
