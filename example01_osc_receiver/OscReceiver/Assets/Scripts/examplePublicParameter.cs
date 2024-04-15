using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class examplePublicParameter : MonoBehaviour
{

    public float parameterFloat = 0.0f; 
    public bool parameterBool = false;

    public TMP_Text monitorText;


    void Start()
    {
        
    }

    void Update()
    {
        Debug.Log("valeur du parametre float : " + parameterFloat);
        Debug.Log("valeur du parametre bool : " + parameterBool);

        monitorText.text = " valeur du parametre float : " + parameterFloat + "\nvaleur du parametre bool : " + parameterBool ;
    }
}
