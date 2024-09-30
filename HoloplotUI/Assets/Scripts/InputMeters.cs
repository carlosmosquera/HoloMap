using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using extOSC;

public class InputMeters : MonoBehaviour
{
    public OSCReceiver OSCReceiver;

    private string _channel1LevelIn = "/channelIn/1";
    private string _channel2LevelIn = "/channelIn/2";
    //private string _channel3LevelIn = "/channelIn/3";


    private Slider channel1In;
    private Slider channel2In;
    //private Slider channel3In;

    // Start is called before the first frame update
    void Start()
    {
       

        channel1In = GameObject.Find("Channel1In").GetComponent<Slider>();
        //channel2In = GameObject.Find("Channel2In").GetComponent<Slider>();



        OSCReceiver.Bind(_channel1LevelIn, ReceivedInput1);
        OSCReceiver.Bind(_channel2LevelIn, ReceivedInput2);


    }

    // -------- INPUT ------------

    public void ReceivedInput1(OSCMessage message)
    {
        //Debug.Log("channel1In" + message);

        if (message.ToFloat(out var value))
        {
            channel1In.value = value;
        }

    }


    public void ReceivedInput2(OSCMessage message)
    {
        //Debug.Log("channel1In" + message);

        if (message.ToFloat(out var value))
        {
            channel2In.value = value;
        }

    }




}
