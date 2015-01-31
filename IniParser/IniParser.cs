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
        #region Properties
        protected const string INHERIT = "Inherit"; // Support section inheritance
        protected const string ROOT = "Root";       // Root section
        protected Dictionary<string, Dictionary<string, string>> keyPairs = new Dictionary<string, Dictionary<string, string>>();
        protected string iniFilePath;
        #endregion

        #region  Constructors, Initializers, Savers
        public IniParser()
        {
        }
        public IniParser(string iniFilePath)
        {
            Read(iniFilePath);
        }
        public void Initialize(string iniStr)
        {
            var lines = iniStr.Split('\n').Where(x => x.Length > 0 && !x.StartsWith(";")).Select(x => x.Trim());
            string currentRoot = ROOT;
            string[] keyPair;
            foreach (var line in lines)
            {
                {
                    if (line != "" && !line.StartsWith(";"))
                    {
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentRoot = line.Substring(1, line.Length - 2);
                            AddSection(currentRoot);
                        }
                        else
                        {
                            keyPair = line.Split(new char[] { '=' }, 2);
                            if (keyPair.Length != 2)
                                continue;
                            AddSetting(currentRoot, keyPair[0], keyPair[1]);
                        }
                    }
                }
            }
        }
        public void Read(string iniFilePath)
        {
            this.iniFilePath = iniFilePath;
            Initialize(File.ReadAllText(iniFilePath));
        }
        public void Save(string iniFilePath = null)
        {
            if (iniFilePath == null)
                iniFilePath = this.iniFilePath;
            File.WriteAllText(iniFilePath, ToString());
        }
        public override string ToString()
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
            return sb.ToString();
        }

        #endregion

        #region Operators: Has, Get, Set, Delete
        public bool HasSetting(string sectionName, string settingName, bool ignoreParent = false)
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
        public bool HasSection(string sectionName)
        {
            return keyPairs.ContainsKey(sectionName);
        }
        public string GetSetting(string sectionName, string settingName, bool ignoreParent = false)
        {
            string ret = null;
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
        public void AddSetting(string sectionName, string settingName, string settingValue)
        {
            AddSection(sectionName);
            keyPairs[sectionName][settingName] = settingValue;
        }
        public void AddSetting(string sectionName, string settingName)
        {
            AddSetting(sectionName, settingName, null);
        }
        public void AddSection(string sectionName)
        {
            if (!keyPairs.ContainsKey(sectionName))
                keyPairs.Add(sectionName, new Dictionary<string, string>());
        }
        public void DeleteSetting(string sectionName, string settingName)
        {
            if (keyPairs.ContainsKey(sectionName) && keyPairs[sectionName].ContainsKey(settingName))
                keyPairs[sectionName].Remove(settingName);
        }

        #endregion

        #region Others: EnumSection, GetAllSettings
        public List<String> EnumSection(string sectionName)
        {
            return keyPairs.Keys.ToList();
        }
        public Dictionary<string, string> GetAllSettings(string sectionName)
        {
            return keyPairs[sectionName];
        }

        #endregion
    }
}