namespace Tools {

    public class StringParsingTool {

        public static string getBetweenCharacter(string input, char startCharacter, char endCharacter) {
            int currentPosition = -1;
            int startPosition = -1, endPosition = -1;
            int consecutiveStartCharacterCount = 0;
            foreach (char c in input) {
                currentPosition++;
                if (c == endCharacter) { // check for end first, in case of identical end and start character
                    consecutiveStartCharacterCount--;
                    if (consecutiveStartCharacterCount < 0) {
                        endPosition = currentPosition; // found the closing character, mark the position
                        break; // no need to continue further in the string
                    }
                } else if (c == startCharacter) {
                    if (startPosition == -1) {
                        startPosition = currentPosition + 1; // found the opening character, mark the position
                    } else consecutiveStartCharacterCount++;
                }
            }

            if (currentPosition == -1 || endPosition == -1)
                return "";

            return input.Substring(startPosition, endPosition - startPosition);
        }

        public static string getNextMacro(string str) {
            int opening = str.IndexOf('@');
            int closing = str.IndexOf(';');
            if (opening > -1 && closing > -1 && opening < closing)
                return str.Substring(opening, 1 + closing - opening);
            else
                return "";
        }

        public static string getBetweenParenhsis(string input) {
            return getBetweenCharacter(input, '(', ')');
        }

        public static string getBetweenCurlyBracket(string input) {
            return getBetweenCharacter(input, '{', '}');
        }

        public static string getBetweenSquareBracket(string input) {
            return getBetweenCharacter(input, '[', ']');
        }

        public static string getBetweenDiamond(string input) {
            return getBetweenCharacter(input, '<', '>');
        }

        public static string getBetweenMacro(string input) {
            return getBetweenCharacter(input, '@', ';');
        }

        public static char firstCharacter(string input) {
            foreach (char c in input) {
                if (c != ' ')
                    return c;
            }
            return ' ';
        }

        public static char lastCharacter(string input) {
            for (int i = input.Length - 1; i >= 0; i--) {
                char c = input[i];
                if (c != ' ')
                    return c;
            }
            return ' ';
        }

        public static string cleanString(string s) {
            string output = s;
            if (output == "")
                return "";
            // clean string start
            while (output.Length > 0) {
                if (output[0] == ' ') { // if whitespace, remove and continue
                    output = output.Substring(1);
                    continue;
                } else {
                    if (output[0] == '"') { // if quote, remove
                        output = output.Substring(1);
                    }
                    break; // if anything but whitespace, stop
                }
            }
            // clean string end
            while (output.Length > 0) {
                if (output[output.Length - 1] == ' ') { // if whitespace, remove and continue
                    output = output.Substring(0, output.Length - 1);
                    continue;
                } else {
                    if (output[output.Length - 1] == '"') { // if quote, remove
                        output = output.Substring(0, output.Length - 1);
                    }
                    break; // if anything but whitespace, stop
                }
            }
            return output;
        }
    }
}
