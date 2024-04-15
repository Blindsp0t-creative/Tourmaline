using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using System;


[Serializable]
public class CompInfo
{
    public CompInfo(Component comp, FieldInfo info)
    {
        infoType = InfoType.Field;
        fieldInfo = info;
        type = fieldInfo.FieldType;
        this.comp = comp;
    }

    public CompInfo(Component comp, PropertyInfo info)
    {
        infoType = InfoType.Property;
        propInfo = info;
        type = propInfo.PropertyType;
        this.comp = comp;
    }

    public CompInfo(Component comp, MethodInfo info)
    {
        infoType = InfoType.Method;
        methodInfo = info;
        this.comp = comp;
    }


    public Component comp;

    public enum InfoType { Property, Field, Method, VFX, Material };
    public InfoType infoType;

    public Type type;
    public FieldInfo fieldInfo;
    public PropertyInfo propInfo;
    public MethodInfo methodInfo;

    public struct GenericInfo
    {
        public GenericInfo(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }
        public Type type;
        public string name;
    };

    public GenericInfo genericInfo;
}


[Serializable]
public class osc_parameter_float
{
    public string name;
    public string fullname;
    public string compName;
    public bool exposed;
    public bool hasRange;
    public float minRange;
    public float maxRange;
    public string type;
    public CompInfo compInfo;
    public float currentValue;
    public int faderId = 0;
}
[Serializable]
public class osc_parameter_int
{
    public string name;
    public string fullname;
    public string compName;
    public bool exposed;
    public bool hasRange;
    public float minRange;
    public float maxRange;
    public string type;
    public CompInfo compInfo;
    public int currentValue;
}
[Serializable]
public class osc_parameter_bool
{
    public string name;
    public string fullname;
    public string compName;
    public bool exposed;
    public string type;
    public CompInfo compInfo;
    public bool currentValue;
}
[Serializable]
public class osc_parameter_string
{
    public string name;
    public string fullname;
    public string compName;
    public bool exposed;
    public string type;
    public CompInfo compInfo;
    public string currentValue;
}



public class oscAutoAssignComponents_tourmaline : MonoBehaviour
{
    public OSC osc;
    public parameters parametres;

    private int[] controllerSliderNbs; 
    private int[] controllerButtonNbs;
    private int[] controllerFaderNbs;

    void Start()
    {
        //register all public parameters from attached components
        scanHierarchy();

        //automaticaly set osc listeners
        osc.SetAllMessageHandler(oscMessageHandler);
    }


    public void scanHierarchy() 
    {
        
        if (parametres == null)
            parametres = new parameters();

        if(parametres.floats == null)
            parametres.floats = new List<osc_parameter_float>();
        if (parametres.ints == null)
            parametres.ints = new List<osc_parameter_int>();
        if (parametres.bools == null)
            parametres.bools = new List<osc_parameter_bool>();
        if (parametres.strings == null)
            parametres.strings = new List<osc_parameter_string>();
               
        //get list of all components attached
        Component[] comps = this.GetComponents<Component>();


        foreach (Component comp in comps)
        {
            //get infos
            FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            int dotIndex = comp.GetType().ToString().LastIndexOf(".");
            string compType = comp.GetType().ToString().Substring(Mathf.Max(dotIndex + 1, 0));

            //for each parameter
            foreach (FieldInfo info in fields)
            {
                if (info.Name != "parametres")
                {
                    //Debug.Log("PARAMETRE DETECTE : " + info.Name + "  > field type: " + info.FieldType.ToString() + " / script component : " + compType);

                    if (info.FieldType.ToString() == "System.Single" || info.FieldType.ToString() == "System.Double")
                    {
                        CompInfo infoT = new CompInfo(comp, info);
                        createParameterFloat(info, compType, true, false, 0, 1, infoT);
                    }
                    else if (info.FieldType.ToString() == "System.Boolean")
                    {
                        CompInfo infoT = new CompInfo(comp, info);
                        createParameterBool(info, compType, true, infoT);
                    }
                    else if (info.FieldType.ToString() == "System.Int32")
                    {
                        CompInfo infoT = new CompInfo(comp, info);                       
                        createParameterInt(info, compType, true, false, 0, 1, infoT);
                    }
                    else if (info.FieldType.ToString() == "System.String")
                    {
                        CompInfo infoT = new CompInfo(comp, info);
                        createParameterString(info, compType, true, infoT);
                    }
                }
            }            
        }
    }
    public void createParameterFloat(FieldInfo info, string compName, bool exposed, bool hasRange, float minRange, float maxRange, CompInfo infoComp)
    {
        osc_parameter_float t = new osc_parameter_float();
        t.name = info.Name;
        t.fullname = info.Name + "_" + compName;
        t.compName = compName;
        t.exposed = exposed;
        t.hasRange = hasRange;
        t.minRange = minRange;
        t.maxRange = maxRange;
        t.type = info.FieldType.ToString();
        t.compInfo = infoComp;

        RangeAttribute rangeAttribute = info.GetCustomAttribute<RangeAttribute>();
        if (rangeAttribute != null)
        {
            t.hasRange = true;
            t.minRange = rangeAttribute.min;
            t.maxRange = rangeAttribute.max;
        }

        parametres.floats.Add(t);
    }
    public void createParameterInt(FieldInfo info, string compName, bool exposed, bool hasRange, float minRange, float maxRange, CompInfo infoComp)
    {
        osc_parameter_int t = new osc_parameter_int();
        t.name = info.Name;
        t.fullname = info.Name + "_" + compName;
        t.compName = compName;
        t.exposed = exposed;
        t.hasRange = hasRange;
        t.minRange = minRange;
        t.maxRange = maxRange;
        t.type = info.FieldType.ToString();
        t.compInfo = infoComp;

        RangeAttribute rangeAttribute = info.GetCustomAttribute<RangeAttribute>();
        if (rangeAttribute != null)
        {
            t.hasRange = true;
            t.minRange = rangeAttribute.min;
            t.maxRange = rangeAttribute.max;
        }

        parametres.ints.Add(t);
    }
    public void createParameterBool(FieldInfo info, string compName, bool exposed, CompInfo infoComp)
    {
        osc_parameter_bool t = new osc_parameter_bool();
        t.name = info.Name;
        t.fullname = info.Name + "_" + compName;
        t.compName = compName;
        t.exposed = exposed;
        t.type = info.FieldType.ToString();
        t.compInfo = infoComp;

        parametres.bools.Add(t);

    }
    public void createParameterString(FieldInfo info, string compName, bool exposed, CompInfo infoComp)
    {
        osc_parameter_string t = new osc_parameter_string();
        t.name = info.Name;
        t.fullname = info.Name + "_" + compName;
        t.compName = compName;
        t.exposed = exposed;
        t.type = info.FieldType.ToString();

        parametres.strings.Add(t);

    }
    
    public void oscMessageHandler(OscMessage message)
    {
        Debug.Log("osc message : " + message.address);
        for (int i = 0; i < parametres.floats.Count; i++)
        {
            if ("/control/" + parametres.floats[i].name == message.address || "/fader/" + parametres.floats[i].name == message.address)
            {
                switch (parametres.floats[i].compInfo.infoType)
                {
                    //float
                    case CompInfo.InfoType.Field:
                            parametres.floats[i].compInfo.fieldInfo.SetValue(parametres.floats[i].compInfo.comp, message.GetFloat(0));
                            parametres.floats[i].currentValue = message.GetFloat(0);
                        
                        break;

                    case CompInfo.InfoType.Property:
                            parametres.floats[i].compInfo.propInfo.SetValue(parametres.floats[i].compInfo.comp, message.GetFloat(0));
                            parametres.floats[i].currentValue = message.GetFloat(0);
                        break;
                }
            }
        }

        for (int i = 0; i < parametres.ints.Count; i++)
        {
            if ("/control/" + parametres.ints[i].name == message.address || "/fader/" + parametres.ints[i].name == message.address)
            {
                switch (parametres.ints[i].compInfo.infoType)
                {
                    //int
                    case CompInfo.InfoType.Field:
                        parametres.ints[i].compInfo.fieldInfo.SetValue(parametres.ints[i].compInfo.comp, message.GetInt(0));
                        parametres.ints[i].currentValue = message.GetInt(0);

                        break;

                    case CompInfo.InfoType.Property:
                        parametres.ints[i].compInfo.propInfo.SetValue(parametres.ints[i].compInfo.comp, message.GetInt(0));
                        parametres.ints[i].currentValue = message.GetInt(0);
                        break;
                }
            }


        }
        for (int i = 0; i < parametres.bools.Count; i++)
        {
            if ("/note/" + parametres.bools[i].name == message.address)
            {
                float val = message.GetInt(0);
                if (val > 0.01)
                {
                    parametres.bools[i].compInfo.fieldInfo.SetValue(parametres.bools[i].compInfo.comp, true);
                }
                else
                {
                    parametres.bools[i].compInfo.fieldInfo.SetValue(parametres.bools[i].compInfo.comp, false);
                }
            }
        }
    }




}
