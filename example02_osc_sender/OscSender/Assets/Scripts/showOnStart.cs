using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class showOnStart : MonoBehaviour
{
    private TMP_Text monitorText;
    void Start()

    {
        monitorText = GetComponent<TMP_Text>();
        monitorText.enabled = true;
    }

}
