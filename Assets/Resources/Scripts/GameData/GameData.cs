using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GameData {
    public string id;

    private object getFieldContent(string fieldName) {
        Dictionary<string, object> FD = (from x in this.GetType().GetProperties() select x).ToDictionary(x => x.Name, x => (x.GetGetMethod().Invoke(this, null) == null ? null : x.GetGetMethod().Invoke(this, null)));
        if (FD.ContainsKey(fieldName))
            return FD[fieldName];
        return null;
    }

    public static object getFieldContent<T>(object o, string fieldName) {
        Dictionary<string, object> FD = (from x in ((T)o).GetType().GetFields() select x).ToDictionary(x => x.Name, x => x.GetValue(o));
        if (FD.ContainsKey(fieldName))
            return FD[fieldName];
        Debug.Log("no field named " + fieldName);
        return null;
    }

}