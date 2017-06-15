using UnityEngine;
using System.Collections.Generic;

public class ObjectJSON {
    public string originalJSON;
    private Dictionary<string, string> data;

    public ObjectJSON(string json) {
        originalJSON = StringParsingTool.getBetweenCurlyBracket(json);
        data = ParserJSON.getDataDictionnary(originalJSON);
        foreach (string s in data.Keys)
            Debug.Log("key : "+ s);
    }

    public bool containsValue(string valueName) {
        return data.ContainsKey(valueName);
    }

    public bool getBool(string valueName, bool error = false) {
        try {
            if (data.ContainsKey(valueName))
                return bool.Parse(data[valueName]);
            return error;
        } catch {
            return error;
        }
    }

    public int getInt(string valueName, int error = 0) {
        try {
            if (data.ContainsKey(valueName))
                return int.Parse(data[valueName]);
            return error;
        } catch {
            return error;
        }
    }

    public float getFloat(string valueName, float error = 0) {
        try {
            if (data.ContainsKey(valueName))
                return float.Parse(data[valueName]);
            return error;
        } catch {
            return error;
        }
    }

    public double getDouble(string valueName, double error = 0) {
        try {
            if (data.ContainsKey(valueName))
                return double.Parse(data[valueName]);
            return error;
        } catch {
            return error;
        }
    }

    public long getLong(string valueName, long error = 0) {
        try {
            if (data.ContainsKey(valueName))
                return long.Parse(data[valueName]);
            return error;
        } catch {
            return error;
        }
    }

    public string getString(string valueName, string error = "") {
        try {
            if (data.ContainsKey(valueName))
                return data[valueName];
            return error;
        } catch {
            return error;
        }
    }
    public ObjectJSON getObjectJSON(string valueName) {
        try {
            if (data.ContainsKey(valueName))
                return new ObjectJSON(data[valueName]);
            return null;
        } catch {
            return null;
        }
    }

    public ArrayJSON getArrayJSON(string valueName) {
        try {
            if (data.ContainsKey(valueName))
                return new ArrayJSON(data[valueName]);
            return null;
        } catch {
            return null;
        }
    }

    public string[] getKeyList() {
        var keys = data.Keys;
        string[] output = new string[keys.Count];
        int i = 0;
        foreach (var e in keys) {
            output[i] = e;
            i++;
        }
        return output;
    }
}