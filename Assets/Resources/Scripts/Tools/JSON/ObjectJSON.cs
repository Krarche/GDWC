using System.Collections.Generic;

namespace Tools.JSON {

    public class ObjectJSON {
        public string originalJSON;
        private Dictionary<string, object> data;

        public ObjectJSON(string json) {
            originalJSON = StringParsingTool.getBetweenCurlyBracket(json);
            Dictionary<string, string> elements = ParserJSON.getDataDictionnary(originalJSON);
            data = new Dictionary<string, object>();
            foreach (string key in elements.Keys) {
                string value = elements[key];
                if (ParserJSON.isStringObjectJSON(value))
                    data[key] = new ObjectJSON(value);
                else if (ParserJSON.isStringArrayJSON(value))
                    data[key] = new ArrayJSON(value);
                else
                    data[key] = StringParsingTool.cleanString(value);
            }
        }

        public bool containsValue(string valueName) {
            return data.ContainsKey(valueName);
        }

        public bool getBool(string valueName, bool error = false) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return bool.Parse((string)data[valueName]);
                return error;
            } catch {
                return error;
            }
        }

        public int getInt(string valueName, int error = 0) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return int.Parse((string)data[valueName]);
                return error;
            } catch {
                return error;
            }
        }

        public float getFloat(string valueName, float error = 0) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return float.Parse((string)data[valueName]);
                return error;
            } catch {
                return error;
            }
        }

        public double getDouble(string valueName, double error = 0) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return double.Parse((string)data[valueName]);
                return error;
            } catch {
                return error;
            }
        }

        public long getLong(string valueName, long error = 0) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return long.Parse((string)data[valueName]);
                return error;
            } catch {
                return error;
            }
        }

        public string getString(string valueName, string error = "") {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is string)
                    return (string)data[valueName];
                return error;
            } catch {
                return error;
            }
        }

        public bool[] getBoolArray(string valueName) {
            ArrayJSON array = getArrayJSON(valueName);
            if (array != null) {
                bool[] output = new bool[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    output[i] = array.getBoolAt(i);
                }
                return output;
            }
            return null;
        }

        public int[] getIntArray(string valueName) {
            ArrayJSON array = getArrayJSON(valueName);
            if (array != null) {
                int[] output = new int[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    output[i] = array.getIntAt(i);
                }
                return output;
            }
            return null;
        }

        public string[] getStringArray(string valueName) {
            ArrayJSON array = getArrayJSON(valueName);
            if (array != null) {
                string[] output = new string[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    output[i] = array.getStringAt(i);
                }
                return output;
            }
            return null;
        }

        public ObjectJSON getObjectJSON(string valueName) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is ObjectJSON)
                    return (ObjectJSON)data[valueName];
                return null;
            } catch {
                return null;
            }
        }

        public ArrayJSON getArrayJSON(string valueName) {
            try {
                if (data.ContainsKey(valueName) && data[valueName] is ArrayJSON)
                    return (ArrayJSON)data[valueName];
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
}