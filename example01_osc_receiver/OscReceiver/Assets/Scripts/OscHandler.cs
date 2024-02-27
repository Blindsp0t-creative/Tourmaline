using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OscHandler : MonoBehaviour
{

    public OSC OSC_Master;
    //public TMP_Text monitorText;

    void Start()
    {
        OSC_Master.SetAllMessageHandler(oscHandlerAll);
        OSC_Master.SetAddressHandler("/Tourmaline/im_up", OnReceiveImUp);
        OSC_Master.SetAddressHandler("/Tourmaline/slider_value", OnReceiveSliderValue);
    }


    void oscHandlerAll(OscMessage message)
    {
        Debug.Log("message OSC recu !! ");
        Debug.Log("adresse du message : " + message.address);

        //monitorText.text = "\n message OSC recu !! \n adresse du message : " + message.address;
    }
    
    void OnReceiveImUp(OscMessage message)
    {
        int number = message.GetInt(0);
        Debug.Log("client n# : " + number + " is up ! ");

        //monitorText.text += "\n\n client n# : " + number + " is up !";
    }

    void OnReceiveSliderValue(OscMessage message)
    {
        float sliderValue = message.GetFloat(0);
        Debug.Log("slider value updated ! " + sliderValue);

        //monitorText.text += "\n\n valeur du slider " + sliderValue ;
    }
}
