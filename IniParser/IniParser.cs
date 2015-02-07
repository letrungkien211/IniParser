using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IniParserLTK
{
    /// <summary>
    /// Simple ini parser: 
    /// 1. Read, save from string or files. 
    /// 2. Section inheritance [A]x=y[B]Inherit=A
    /// 3. Extensible, all methods are virtual
    /// 4. Comment out in .ini file by using #
    /// </summary>
    public class IniParser
    {
        #region Protected Properties
        protected const string INHERIT = "Inherit"; // Support section inheritance
        protected const string ROOT = "Root";       // Root section
        protected Dictionary<string, Dictionary<string, string>> keyPairs = new Dictionary<string, Dictionary<string, string>>();  // dictionary to hold all sections and settings
        protected string iniFilePath;  // path of the ini file
        #endregion

        #region  Constructors, Initializers, Savers

        /// <summary>
        /// Empty constructor
        /// </summary>
        public IniParser()
        {
        }

        /// <summary>
        /// Construct parser from an ini file
        /// </summary>
        /// <param name="iniFilePath">Path of an ini file</param>
        public IniParser(string iniFilePath)
        {
            Read(iniFilePath);
        }

        /// <summary>
        /// Initialize from an ini string. 
        /// </summary>
        /// <param name="iniStr">String ini</param>
        /// <param name="clearExistingSettings">If true, all existing settings will be cleared.</param>
        public virtual void Initialize(string iniStr, bool clearExistingSettings = true)
        {
            if (clearExistingSettings)
                DeleteAll();
            var lines = iniStr.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0 && !x.StartsWith("#"));
            var currentRoot = ROOT;
            string[] keyPair;
            foreach (var line in lines)
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

        /// <summary>
        /// Convert settings to string
        /// </summary>
        /// <returns>String contains all settings</returns>
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

        /// <summary>
        /// Read ini settings from a file
        /// </summary>
        /// <param name="iniFilePath">Path of an ini file</param>
        /// <param name="clearExistingSettings">If true, all existing settings will be cleared. If false, all current settings will be kept and overwritten by new value if the pair (section, setting name) matches.</param>
        public virtual void Read(string iniFilePath, bool clearExistingSettings = true)
        {
            this.iniFilePath = iniFilePath;
            Initialize(File.ReadAllText(iniFilePath), clearExistingSettings);
        }
        /// <summary>
        /// Read ini settings from a textreader or streamreader
        /// </summary>
        /// <param name="iniFilePath">Path of an ini file</param>
        /// <param name="clearExistingSettings">If true, all existing settings will be cleared. If false, all current settings will be kept and overwritten by new value if the pair (section, setting name) matches.</param>  
        public virtual void Read(TextReader reader, bool clearExistingSettings = true)
        {
            Initialize(reader.ReadToEnd(), clearExistingSettings);
        }

        /// <summary>
        /// Save to a file
        /// </summary>
        /// <param name="iniFilePath">Path of an ini file. If not specified, the most recent file used for reading ini setting will be used.</param>
        public virtual void Save(string iniFilePath = null)
        {
            if (iniFilePath == null)
                iniFilePath = this.iniFilePath;
            File.WriteAllText(iniFilePath, ToString());
        }
        #endregion

        #region Operators: Has, Get, Set, Delete
        /// <summary>
        /// Check if a setting exists under section.
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        /// <param name="settingName">Setting's name</param>
        /// <param name="ignoreParent">True: Ignore Inheritance. False: Get setting from parent section if this section does not contain the setting</param>
        /// <returns>True/False</returns>
        public virtual bool HasSetting(string sectionName, string settingName, bool ignoreParent = false)
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
        /// Check if a section exists
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        /// <returns>True/False</returns>
        public virtual bool HasSection(string sectionName)
        {
            return keyPairs.ContainsKey(sectionName);
        }

        /// <summary>
        /// Get a setting under a section.
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        /// <param name="settingName">Setting's name</param>
        /// <param name="ignoreParent">True: Ignore Inheritance. False: Get setting from parent section if this section does not contain the setting</param>
        /// <returns>String</returns>
        public virtual string GetSetting(string sectionName, string settingName, bool ignoreParent = false)
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

        /// <summary>
        /// Get all settings under a section
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        /// <returns>Dictionary of settings for a section</returns>
        public virtual Dictionary<string, string> GetAllSettings(string sectionName)
        {
            return keyPairs[sectionName];
        }

        /// <summary>
        /// Add a setting to a section
        /// </summary>
        /// <param name="sectionName">Sectin's name</param>
        /// <param name="settingName">Setting's name</param>
        /// <param name="settingValue">Setting's value</param>
        public virtual void AddSetting(string sectionName, string settingName, string settingValue)
        {
            AddSection(sectionName);
            keyPairs[sectionName][settingName] = settingValue;
        }

        /// <summary>
        /// Add a section
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        public virtual void AddSection(string sectionName)
        {
            if (!keyPairs.ContainsKey(sectionName))
                keyPairs.Add(sectionName, new Dictionary<string, string>());
        }

        /// <summary>
        /// Delete a section in the settings
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        public virtual void DeleteSection(string sectionName)
        {
            if (HasSection(sectionName))
            {
                keyPairs[sectionName].Clear();
            }
            keyPairs.Remove(sectionName);
        }

        /// <summary>
        /// Delete a setting in a section
        /// </summary>
        /// <param name="sectionName">Section's name</param>
        /// <param name="settingName">Setting's name</param>
        public virtual void DeleteSetting(string sectionName, string settingName)
        {
            if (keyPairs.ContainsKey(sectionName) && keyPairs[sectionName].ContainsKey(settingName))
                keyPairs[sectionName].Remove(settingName);
        }

        /// <summary>
        /// Delete all sections and settings
        /// </summary>
        public virtual void DeleteAll()
        {
            foreach (var key in keyPairs)
            {
                key.Value.Clear();
            }
            keyPairs.Clear();
        }
        #endregion

        #region Others: EnumSection
        /// <summary>
        /// Enumerate all the sections
        /// </summary>
        /// <returns>A list of section settings</returns>
        public virtual List<String> EnumSection()
        {
            return keyPairs.Keys.ToList();
        }
        #endregion
    }
}