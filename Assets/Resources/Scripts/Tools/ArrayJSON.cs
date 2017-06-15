using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArrayJSON : IEnumerable<ObjectJSON> {
    public string originalJSON;

    ObjectJSON[] data;

    public ArrayJSON(string json) {
        originalJSON = StringParsingTool.getBetweenSquareBracket(json);
        string[] elements = ParserJSON.getDataList(originalJSON);
        data = new ObjectJSON[elements.Length];
        for (int i = 0; i < elements.Length; i++) {
            data[i] = new ObjectJSON(elements[i]);
        }
    }

    ObjectJSON getObjectAt(int index) {
        if (index < 0 || index >= data.Length)
            return null;
        return data[index];
    }

    public int Length {
        get { return data.Length; }
    }

    public ObjectJSON this[int index] {
        get {
            return getObjectAt(index);
        }
    }

    public IEnumerator<ObjectJSON> GetEnumerator() {
        return ((IEnumerable<ObjectJSON>)data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<ObjectJSON>)data).GetEnumerator();
    }
}
