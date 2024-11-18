using System;
using System.Collections;
using System.Collections.Generic;

namespace Contrib.StopWords;

/// <summary>
/// DFA Algorithm for Finite State Automaton
/// M = (Q, E, f, q0, Z)
/// Q = Set of states - All node dictionaries plus an empty set
/// E = Input character set - Set of individual characters in sensitive words
/// f = Transition function - Index relationship between nodes (including empty set) indexed by characters in E
/// q0 = Initial state - Empty set, returns self for any character index
/// Z = Output state set - All sensitive words
/// Complexity O(n log n) where n is length of input text
/// </summary>
internal class WordNode
{
    private readonly Dictionary<int, WordNode> node;

    private byte _isEnd = 0;

    public WordNode()
    {
        node = new Dictionary<int, WordNode>();
    }

    public void Add(string word)
    {
        if (word.Length <= 0) return;
        var key = (int)word[0];
        if (!node.ContainsKey(key))
        {
            node.Add(key, new WordNode());
        }

        var subNode = node[key];
        if (word.Length > 1)
        {
            subNode.Add(word.Substring(1));
        }
        else
        {
            subNode._isEnd = 1;
        }
    }

    public int CheckAndGetEndIndex(string sourceDBCText, int cursor, Func<char, bool>? checkSpecialSym)
    {
        // Check if the next character is not Chinese, number, or letter. If not, add 1 to the offset.
        for (var i = cursor; i < sourceDBCText.Length; i++)
        {
            if (checkSpecialSym != null && checkSpecialSym(sourceDBCText[i]))
            {
                cursor++;
            }
            else break;
        }

        if (cursor >= sourceDBCText.Length)
        {
            return -1;
        }

        int key = sourceDBCText[cursor];
        if (node.ContainsKey(key))
        {
            var group = node[key];
            if (group._isEnd == 1)
            {
                return cursor;
            }

            return node[key].CheckAndGetEndIndex(sourceDBCText, cursor + 1, checkSpecialSym);
        }

        return -1;
    }
}

/// <summary>
/// Sensitive word tool
/// <para>Reference material: http://mot.ttthyy.com/328.html</para>
/// <para>This version is optimized from the original code.</para>
/// </summary>
public class StopWordsUtils
{
    private WordNode? _wordNodes;

    public void InitDictionary(string[] words)
    {
        if (_wordNodes != null) return;

        _wordNodes = new WordNode();
        for (int i = 0, Imax = words.Length; i < Imax; i++)
        {
            string word = words[i];
            string dbcWord = this.ToDBC(word);

            // Convert to Traditional Chinese characters
            //wordList.Add(Microsoft.VisualBasic.Strings.StrConv(key, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0));
            if (dbcWord.Length > 0)
            {
                _wordNodes.Add(word);
            }
        }
    }

    private struct RNode
    {
        public int start;
        public int len;
        public int type; //0为敏感词替换 1为数字序列替换
    }

    public bool CheckBadWord(string sourceText, int filterNum = 0)
    {
        if (string.IsNullOrWhiteSpace(sourceText)) return false;

        var sourceDBCText = ToDBC(sourceText);
        for (var i = 0; i < sourceDBCText.Length; i++)
        {
            var badWordLen = 0;
            if (filterNum > 0 && IsNum(sourceDBCText[i]))
            {
                badWordLen = CheckNumberSeq(sourceDBCText, i, filterNum);
                if (badWordLen > 0)
                {
                    return true;
                }
            }

            // Search for word groups starting with this character
            badWordLen = Check(sourceDBCText, i);
            if (badWordLen > 0)
            {
                return true;
            }
        }
        //double timeCost = (System.DateTime.Now - timeStart).TotalMilliseconds;
        //Debug.LogError("timeCost:" + timeCost + " ms");

        return false;
    }


    public string FilterWithChar(string sourceText, char replaceChar, int filterNum = 0, string numReplace = null)
    {
        //var timeStart = System.DateTime.Now;
        if (sourceText != string.Empty)
        {
            var sourceDBCText = ToDBC(sourceText);

            char[] tempString = sourceText.ToCharArray();
            List<RNode> replaceList = new List<RNode>();

            for (int i = 0; i < sourceDBCText.Length; i++)
            {
                int badWordLen = 0;
                if (filterNum > 0 && IsNum(sourceDBCText[i]))
                {
                    badWordLen = CheckNumberSeq(sourceDBCText, i, filterNum);
                    if (badWordLen > 0)
                    {
                        badWordLen = badWordLen + 1;
                        if (numReplace == null)
                        {
                            for (int pos = 0; pos < badWordLen; pos++)
                            {
                                tempString[pos + i] = replaceChar;
                            }
                        }
                        else
                        {
                            replaceList.Add(new RNode { start = i, len = badWordLen, type = 1 });
                        }

                        i = i + badWordLen - 1;
                        continue;
                    }
                }

                // Search for word groups starting with this character
                badWordLen = Check(sourceDBCText, i);
                if (badWordLen > 0)
                {
                    for (int pos = 0; pos < badWordLen; pos++)
                    {
                        tempString[pos + i] = replaceChar;
                    }

                    i = i + badWordLen - 1;
                }
            }

            string result;
            if (replaceList.Count > 0)
            {
                result = ReplaceString(tempString, replaceList, null, numReplace);
            }
            else
            {
                result = new string(tempString);
            }

            //double timeCost = (System.DateTime.Now - timeStart).TotalMilliseconds;
            //Debug.LogError("timeCost:" + timeCost + " ms");
            return result;
        }
        else
        {
            return string.Empty;
        }
    }

    public string FilterWithStr(string sourceText, string replaceStr, int filterNum = 0, string numReplace = null)
    {
        //var timeStart = System.DateTime.Now;
        if (sourceText != string.Empty)
        {
            string sourceDBCText = ToDBC(sourceText);
            List<RNode> replaceList = new List<RNode>();

            if (filterNum > 0 && numReplace == null)
            {
                numReplace = replaceStr;
            }

            int badWordLen = 0;
            for (int i = 0; i < sourceDBCText.Length; i++)
            {
                if (filterNum > 0 && IsNum(sourceDBCText[i]))
                {
                    badWordLen = CheckNumberSeq(sourceDBCText, i, filterNum);
                    if (badWordLen > 0)
                    {
                        badWordLen = badWordLen + 1;
                        int start = i;
                        replaceList.Add(new RNode { start = start, len = badWordLen, type = 1 });
                        i = i + badWordLen - 1;
                        continue;
                    }
                }

                badWordLen = Check(sourceDBCText, i);
                if (badWordLen > 0)
                {
                    replaceList.Add(new RNode { start = i, len = badWordLen, type = 0 });
                    i = i + badWordLen - 1;
                }
            }

            string tempStr = ReplaceString(sourceText.ToCharArray(), replaceList, replaceStr, numReplace);
            //double timeCost = (System.DateTime.Now - timeStart).TotalMilliseconds;
            //Debug.LogError("timeCost:" + timeCost + " ms");
            return tempStr;
        }
        else
        {
            return string.Empty;
        }
    }

    private static string ReplaceString(char[] charArry, List<RNode> nodes, string replaceStr, string numReplace)
    {
        if (string.IsNullOrWhiteSpace(numReplace))
        {
            numReplace = replaceStr;
        }

        if (string.IsNullOrWhiteSpace(replaceStr))
        {
            replaceStr = numReplace;
        }

        List<char> charList = new List<char>(charArry);
        int offset = 0;
        for (int i = 0, Imax = nodes.Count; i < Imax; i++)
        {
            int start = nodes[i].start + offset;
            int len = nodes[i].len;
            int endIndex = start + len - 1;
            string str = nodes[i].type == 0 ? replaceStr : numReplace;

            if (str.Length < len)
            {
                charList.RemoveRange(start, len - str.Length);
            }

            for (int j = 0, Jmax = str.Length; j < Jmax; j++)
            {
                char ch = str[j];
                int index = start + j;
                if (index <= endIndex)
                {
                    charList[index] = ch;
                }
                else
                {
                    charList.Insert(index, ch);
                }
            }

            offset += str.Length - len;
        }

        return new string(charList.ToArray());
    }

    private int Check(string sourceText, int cursor)
    {
        var endsor = _wordNodes!.CheckAndGetEndIndex(sourceText, cursor, CheckSpecialSym);
        var wordLength = endsor >= cursor ? endsor - cursor + 1 : 0;
        return wordLength;
    }

    private static int CheckNumberSeq(string sourceText, int cursor, int filterNum)
    {
        var count = 0;
        const int offset = 0;
        if (cursor + 1 >= sourceText.Length)
        {
            return 0;
        }

        // Check if the next character is not Chinese, number, or letter. If not, add 1 to the offset.
        for (int i = cursor + 1; i < sourceText.Length; i++)
        {
            //if(checkSpecialSym(sourceText[i]))
            //{
            //    offset++;
            //}
            //else 
            if (!IsNum(sourceText[i]))
            {
                break;
            }
            else
            {
                count++;
            }
        }

        if (count + 1 >= filterNum)
        {
            var wordLength = count + offset;
            return wordLength;
        }
        else
        {
            return 0;
        }
    }

    private static bool CheckSpecialSym(char character)
    {
        return !IsChinese(character) && !IsNum(character) && !IsAlphabet(character);
    }

    private static bool IsChinese(char character)
    {
        // The range of Chinese ideographic characters is 4E00-9FA5.
        var charVal = (int)character;
        return charVal >= 0x4e00 && charVal <= 0x9fa5;
    }

    private static bool IsNum(char character)
    {
        var charVal = (int)character;
        return charVal >= 48 && charVal <= 57;
    }

    private static bool IsAlphabet(char character)
    {
        var charVal = (int)character;
        return ((charVal >= 97 && charVal <= 122) || (charVal >= 65 && charVal <= 90));
    }


    /// <summary>
    /// Function to convert full-width characters to half-width characters (DBC case)
    /// </summary>
    /// <param name="input">Any string input</param>
    /// <returns>String converted to half-width characters</returns>
    /// <remarks>
    /// Full-width space is represented by 12288, half-width space by 32
    /// Other character conversion (33-126): subtract 65248 from both full-width and half-width representations
    /// </remarks>
    private string ToDBC(string input)
    {
        var c = input.ToCharArray();
        for (var i = 0; i < c.Length; i++)
        {
            if (c[i] == 12288)
            {
                c[i] = (char)32;
                continue;
            }

            if (c[i] > 65280 && c[i] < 65375)
                c[i] = (char)(c[i] - 65248);
        }

        return new string(c).ToLower();
    }
}