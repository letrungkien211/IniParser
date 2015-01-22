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
        /// <summary>
        /// Protected variables
        /// </summary>
        protected const string INHERIT = "Inherit";
        protected const string ROOT = "Root";
        protected Dictionary<string, Dictionary<string, string>> keyPairs = new Dictionary<string, Dictionary<string, string>>();
        protected String iniFilePath;

        public IniParser()
        {

        }
        public IniParser(String iniFilePath)
        {
            this.iniFilePath = iniFilePath;
            Initialize(File.ReadAllText(iniFilePath));
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

            try
            {
                File.WriteAllText(newFilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveSettings()
        {
            if (iniFilePath == null)
                throw new Exception("Please specify the path as default save path has not been set");
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
