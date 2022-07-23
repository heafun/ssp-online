using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ApiSettings", menuName ="ScriptableObject/ApiSettings")]
public class ApiSettings : ScriptableObject
{
    public string apiBaseUrl;
    public string[] headers;
}
