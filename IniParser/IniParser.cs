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
        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
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

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
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

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public List<String> EnumSection(String sectionName)
        {
            return keyPairs.Keys.ToList();
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        /// <param name="settingValue">Value of key.</param>
        public void AddSetting(String sectionName, String settingName, String settingValue)
        {
            AddSection(sectionName);
            keyPairs[sectionName][settingName] = settingValue;
        }

        /// <summary>
        /// Adds or replaces a setting to the table to be saved with a null value.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void AddSetting(String sectionName, String settingName)
        {
            AddSetting(sectionName, settingName, null);
        }

        /// <summary>
        /// Remove a setting.
        /// </summary>
        /// <param name="sectionName">Section to add under.</param>
        /// <param name="settingName">Key name to add.</param>
        public void DeleteSetting(String sectionName, String settingName)
        {
            if (keyPairs.ContainsKey(sectionName) && keyPairs[sectionName].ContainsKey(settingName))
                keyPairs[sectionName].Remove(settingName);
        }

        /// <summary>
        /// Save settings to new file.
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
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

        /// <summary>
        /// Save settings back to ini file.
        /// </summary>
        public void SaveSettings()
        {
            SaveSettings(iniFilePath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="settingName"></param>
        /// <param name="ignoreParent"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public bool HasSection(String sectionName)
        {
            return keyPairs.ContainsKey(sectionName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetAllSettings(string sectionName)
        {
            return keyPairs[sectionName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionName"></param>
        public void AddSection(String sectionName)
        {
            if (!keyPairs.ContainsKey(sectionName))
                keyPairs.Add(sectionName, new Dictionary<string, string>());
        }
    }


}
