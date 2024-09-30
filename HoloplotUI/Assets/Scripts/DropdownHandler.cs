using UnityEngine;
using UnityEngine.UI;
using TMPro;
using extOSC;

public class DropdownHandler : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public GameObject zone1;
    public GameObject zone2;
    public GameObject zone3;
    public GameObject zone4;


    public GameObject metersZone1;
    public GameObject metersZone2;
    public GameObject metersZone3;


    public OSCTransmitter OSCTransmitter;
    private string _zone1Address = "/zone1/";
    private string _zone2Address = "/zone2/";
    private string _zone3Address = "/zone3/";

    private SpriteRenderer spriteZone1;
    private SpriteRenderer spriteZone2;
    private SpriteRenderer spriteZone3;



    void Start()
    {
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });


        spriteZone1 = zone1.GetComponent<SpriteRenderer>();
        spriteZone2 = zone2.GetComponent<SpriteRenderer>();
        spriteZone3 = zone3.GetComponent<SpriteRenderer>();




        metersZone2.SetActive(false);
        //zone3.SetActive(false);

        // Initialize to make sure only the first object is active at the start
        DropdownValueChanged(dropdown);
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                metersZone1.SetActive(true);
                metersZone2.SetActive(false);
                metersZone3.SetActive(false);
                //metersZone4.SetActive(false);

                zone1.SetActive(true);
                zone2.SetActive(true);
                zone3.SetActive(true);
                zone4.SetActive(false);


                spriteZone1.color = Color.green;
                spriteZone2.color = Color.green;
                spriteZone3.color = Color.green;



                //var messageZone1 = new OSCMessage(_zone1Address);
                //messageZone1.AddValue(OSCValue.Int(1));
                //OSCTransmitter.Send(messageZone1);

                break;
            case 1:
                metersZone1.SetActive(false);
                metersZone2.SetActive(true);
                metersZone3.SetActive(false);
                //metersZone4.SetActive(false);

                zone1.SetActive(true);
                zone2.SetActive(true);
                zone3.SetActive(true);
                zone4.SetActive(false);


                spriteZone1.color = Color.grey;
                spriteZone2.color = Color.green;
                spriteZone3.color = Color.grey;


                //var messageZone2 = new OSCMessage(_zone2Address);
                //messageZone2.AddValue(OSCValue.Int(1));
                //OSCTransmitter.Send(messageZone2);


                break;
            case 2:
                metersZone1.SetActive(false);
                metersZone2.SetActive(false);
                metersZone3.SetActive(true);
                //metersZone4.SetActive(false);

                zone1.SetActive(true);
                zone2.SetActive(true);
                zone3.SetActive(true);
                zone4.SetActive(false);

                spriteZone1.color = Color.grey;
                spriteZone2.color = Color.grey;
                spriteZone3.color = Color.green;


                //var messageZone3 = new OSCMessage(_zone3Address);
                //messageZone3.AddValue(OSCValue.Int(1));
                //OSCTransmitter.Send(messageZone3);
                break;

            case 3:
                zone1.SetActive(false);
                zone2.SetActive(false);
                zone3.SetActive(false);
                zone4.SetActive(true);





                //var messageZone3 = new OSCMessage(_zone3Address);
                //messageZone3.AddValue(OSCValue.Int(1));
                //OSCTransmitter.Send(messageZone3);
                break;
        }
    }
}
