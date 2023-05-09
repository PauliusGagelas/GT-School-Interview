using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DataObject
{
    public int id;
    public string subject;
    public string grade;
    public int mastery;
    public string domainid;
    public string domain;
    public string cluster;
    public string standardid;
    public string standarddescription;

    // Used when graphical objects are rendered
    public JengaPiece objectReference;
}


public class RootObject
{
    public List<DataObject> dataObjects;
}
