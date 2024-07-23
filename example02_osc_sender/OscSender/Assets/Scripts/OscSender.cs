using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class OscSender : MonoBehaviour
{
    public OSC master;
    public TMP_Text monitorText;

    private int nbMessage=1;

    public void Start()
    {

    }

    public void sendMessageHelloWorld()
    {
        OscMessage msg = new OscMessage();
        msg.address = "/Tourmaline/im_up";
        nbMessage++;
        msg.values.Add(nbMessage);
        master.Send(msg);
                
        monitorText.text = "\n " + nbMessage + " messages OSC envoyé. ";
    }

    public void sendMessagePlaySound()
    {
        OscMessage msg = new OscMessage();
        msg.address = "/Tourmaline/playsound";
        master.Send(msg);
                
        monitorText.text = "\n trigger sound message.";
    }

    public void sendMessageShowVideo()
    {
        OscMessage msg = new OscMessage();
        msg.address = "/Tourmaline/showvideo";
        master.Send(msg);
                
        monitorText.text = "\n trigger video message.";
    }
}

