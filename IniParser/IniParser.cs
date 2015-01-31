using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniParserLTK
{
    public class IniParser
    {
        protected const string INHERIT = "Inherit"; // Support section inheritance
        protected const string ROOT = "Root";       // Root section
        protected Dictionary<string, Dictionary<string, string>> keyPairs = new Dictionary<string, Dictionary<string, string>>();
        protected String iniFilePath;   // The file path
        public IniParser(String iniPath)
        {
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;

            iniFilePath = iniPath;

            using (var iniFile = new StreamReader(iniPath))
            {
                while ((strLine = iniFile.ReadLine()) != null)
                {
                    strLine = strLine.Trim();
                    if (strLine != "" && !strLine.StartsWith(";"))
                    {
                        if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                        {
                            currentRoot = strLine.Substring(1, strLine.Length - 2);
                            AddSection(currentRoot);
                        }
                        else
                        {
                            keyPair = strLine.Split(new char[] { '=' }, 2);
                            if (keyPair.Length != 2)
                                continue;
                            if (currentRoot == null)
                                currentRoot = ROOT;
                            AddSetting(currentRoot, keyPair[0], keyPair[1]);
                        }
                    }
                }
            }
        }

        public String GetSetting(String sectionName, String settingName, bool ignoreParent = false)
        {
            String ret = null;
            if (keyPairs.ContainsKey(sectionName))
            {
                if (keyPairs[sectionName].ContainsKey(settingName))
                {
                    ret = keyPairs[sectionName][settingName];
                }
                else if (!ignoreParent && keyPairs[sectionName].ContainsKey(INHERIT))
                {
                    ret = GetSetting(keyPairs[sectionName][INHERIT], settingName);
                }
            }
            return ret;
        }

        public List<String> EnumSection(String sectionName)
        {
            return keyPairs.Keys.ToList();
        }

        public void AddSetting(String sectionName, String settingName, String settingValue)
        {
            AddSection(sectionName);
            keyPairs[sectionName][settingName] = settingValue;
        }

        public void AddSetting(String sectionName, String settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        public void DeleteSetting(String sectionName, String settingName)
        {
            if (keyPairs.ContainsKey(sectionName) && keyPairs[sectionName].ContainsKey(settingName))
                keyPairs[sectionName].Remove(settingName);
        }

        public void SaveSettings(String newFilePath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var section in keyPairs)
            {
                sb.Append("[" + section.Key + "]\r\n");

                foreach (var kv in section.Value)
                {
                    sb.Append(kv.Key + "=" + kv.Value + "\r\n");
                }
            }
            File.WriteAllText(newFilePath, sb.ToString());
        }

        public void SaveSettings()
        {
            SaveSettings(iniFilePath);
        }

        public bool HasSetting(String sectionName, String settingName, bool ignoreParent = false)
        {
            bool flag = false;
            if (keyPairs.ContainsKey(sectionName))
            {
                if (keyPairs[sectionName].ContainsKey(settingName))
                {
                    flag = true;
                }
                else if (!ignoreParent && keyPairs[sectionName].ContainsKey(INHERIT))
                {
                    flag = HasSetting(keyPairs[sectionName][INHERIT], settingName);
                }
            }
            return flag;
        }

        public bool HasSection(String sectionName)
        {
            return keyPairs.ContainsKey(sectionName);
        }

        public Dictionary<string, string> GetAllSettings(string sectionName)
        {
            return keyPairs[sectionName];
        }

        public void AddSection(String sectionName)
        {
            if (!keyPairs.ContainsKey(sectionName))
                keyPairs.Add(sectionName, new Dictionary<string, string>());
        }
    }
}
