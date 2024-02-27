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
        OscMessage msg = new OscMessage();
        msg.address = "/Tourmaline/im_up";
        msg.values.Add(1);
        master.Send(msg);

        monitorText.text = "\n " + nbMessage + " message OSC envoyé. ";
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
}

