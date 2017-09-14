using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools.JSON {

    public class ParserJSON {

        public static ObjectJSON getObjectJSONFromFile(string filePath) {
            try {
                StreamReader reader = new StreamReader(filePath, Encoding.Default);
                string json = reader.ReadToEnd();
                return new ObjectJSON(json);
            } catch {
                return null;
            }
        }

        public static ObjectJSON getObjectJSONFromAsset(string assetName) {
            TextAsset asset = Resources.Load(assetName) as TextAsset;
            //Debug.Log(asset.text);
            return new ObjectJSON(cleanJSON(asset.text));
        }

        public static string cleanJSON(string json) {
            return json.Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace("\\n", "\n");
        }

        public static string[] getDataList(string json) {
            List<string> dataList = new List<string>();
            int startPos = 0;
            Stack<char> nesters = new Stack<char>();
            for (int i = 0; i < json.Length; i++) {
                char c = json[i];
                if (c == '\\') { // skip escaped character
                    i++;
                    continue;
                }
                if (nesters.Count > 0 && c == getClosingChar(nesters.Peek())) { // we are nested, and are nesting out
                    nesters.Pop();
                    continue;
                }
                if (isOpeningChar(c)) { // nesting
                    nesters.Push(c);
                    continue;
                }
                if (nesters.Count == 0 && c == ',') { // end of current data, cut here
                    string newEntry = json.Substring(startPos, i - startPos);
                    dataList.Add(newEntry);
                    startPos = i + 1;
                }
            }
            string lastEntry = json.Substring(startPos, json.Length - startPos);
            dataList.Add(lastEntry);
            return dataList.ToArray();
        }

        public static string[] getKeyValue(string json) {
            List<string> dataList = new List<string>();
            bool inString = false;
            string key = json;
            string value = "";
            for (int i = 0; i < json.Length; i++) {
                char c = json[i];
                if (c == '\\') { // skip escaped character
                    i++;
                    continue;
                }
                if (!inString) {
                    if (c == '"') { // entering string
                        inString = true;
                        continue;
                    }
                    if (c == ':') {
                        key = json.Substring(0, i);
                        value = json.Substring(i + 1);
                        break;
                    }
                } else {
                    if (c == '"') { // leaving string
                        inString = false;
                        continue;
                    }
                }
            }
            key = StringParsingTool.cleanString(key);
            value = StringParsingTool.cleanString(value);
            dataList.Add(key);
            dataList.Add(value);
            return dataList.ToArray();
        }

        public static Dictionary<string, string> getDataDictionnary(string json) {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string[] dataList = getDataList(json);
            foreach (string s in dataList) {
                string[] keyValue = getKeyValue(s);
                string key = keyValue[0], value = keyValue[1];
                output[key] = value;
            }
            return output;
        }

        public static bool isOpeningChar(char openingChar) {
            switch (openingChar) {
                case '{':
                    return true;
                case '[':
                    return true;
                case '(':
                    return true;
                case '"':
                    return true;
                default:
                    return false;
            }
        }

        public static char getClosingChar(char openingChar) {
            switch (openingChar) {
                case '{':
                    return '}';
                case '[':
                    return ']';
                case '(':
                    return ')';
                case '"':
                    return '"';
                default: return '\a';
            }
        }

        public static bool isStringObjectJSON(string input) {
            return StringParsingTool.firstCharacter(input) == '{' && StringParsingTool.lastCharacter(input) == '}';
        }

        public static bool isStringArrayJSON(string input) {
            return StringParsingTool.firstCharacter(input) == '[' && StringParsingTool.lastCharacter(input) == ']';
        }

        public static bool isStringValueJSON(string input) {
            return StringParsingTool.firstCharacter(input) == '"' && StringParsingTool.lastCharacter(input) == '"';
        }
    }
}


