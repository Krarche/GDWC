using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tools.JSON {

    public class ArrayJSON : IEnumerable<object> {
        public string originalJSON;

        object[] data;

        public ArrayJSON(string json) {
            originalJSON = StringParsingTool.getBetweenSquareBracket(json);
            string[] elements = ParserJSON.getDataList(originalJSON);
            data = new object[elements.Length];
            for (int i = 0; i < elements.Length; i++) {
                string value = elements[i];
                if (ParserJSON.isStringObjectJSON(value))
                    data[i] = new ObjectJSON(value);
                else if (ParserJSON.isStringArrayJSON(value))
                    data[i] = new ArrayJSON(value);
                else
                    data[i] = StringParsingTool.cleanString(value);
            }
        }

        public ObjectJSON getObjectJSONAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is ObjectJSON)
                return (ObjectJSON)at;
            return null;
        }

        public ArrayJSON getArrayJSONAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is ArrayJSON)
                return (ArrayJSON)at;
            return null;
        }

        public int getIntAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return int.Parse((string)at);
            return 0;
        }

        public float getFloatAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return float.Parse((string)at);
            return 0;
        }

        public double getDoubleAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return double.Parse((string)at);
            return 0;
        }

        public long getLongAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return long.Parse((string)at);
            return 0;
        }

        public bool getBoolAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return bool.Parse((string)at);
            return false;
        }

        public string getStringAt(int index) {
            object at = getObjectAt(index);
            if (at != null && at is string)
                return (string)at;
            return "";
        }

        public object getObjectAt(int index) {
            if (index < 0 || index >= data.Length)
                return null;
            return data[index];
        }

        public int Length {
            get { return data.Length; }
        }

        public object this[int index] {
            get {
                return getObjectAt(index);
            }
        }

        public IEnumerator<object> GetEnumerator() {
            return ((IEnumerable<object>)data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<object>)data).GetEnumerator();
        }
    }
}