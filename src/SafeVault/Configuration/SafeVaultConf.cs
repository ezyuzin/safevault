/*
  SafeVault KeePass Plugin
  Copyright (C) 2016-2017 Evgeny Zyuzin <evgeny.zyuzin@gmail.com>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using SafeVault.Exceptions;

namespace SafeVault.Configuration
{
    internal sealed class SafeVaultConf
    {
        public const string CONFFILE_EXTENSION = ".vault.xml";

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string Type { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string Version { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string ServerUrl { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string ServerCertificateName { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string ClientCertificateName { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig, FieldName = "VaultUsername")]
        public string Username { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig, FieldName = "VaultKeyname")]
        public string VaultKeyname { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string DatabaseKeyA { get; set; }

        [ConfStorage(Type = ConfStorageType.LocalConfig)]
        public string Salt { get; set; }

        [ConfStorage(Type = ConfStorageType.PwDatabase)]
        public ProtectedString SyncPassword { get; set; }

        [ConfStorage(Type = ConfStorageType.PwDatabase)]
        public AutoSyncMode AutoSyncMode { get; set; }

        [ConfStorage(Type = ConfStorageType.PwDatabase)]
        public string SyncRemoteLastModified { get; set; }

        private PwDatabase _pwDatabase;
        private string _localFilename;

        public SafeVaultConf()
        {
        }

        public SafeVaultConf(PwDatabase pwDatabase)
            : this(pwDatabase.IOConnectionInfo)
        {
            _pwDatabase = pwDatabase;
            ReadFromPwDatabase();
        }

        public SafeVaultConf(IOConnectionInfo ioc1)
        {
            _localFilename = UrlUtil.StripExtension(ioc1.Path) + CONFFILE_EXTENSION;
            ReadFromLocalConf();
        }

        public void ChangeDatabase(PwDatabase db)
        {
            _pwDatabase = db;
            _localFilename = UrlUtil.StripExtension(db.IOConnectionInfo.Path) + CONFFILE_EXTENSION;
        }

        public void Save()
        {
            try
            {
                SaveToPwDatabase();
            }
            catch (Exception e)
            {
                throw new ConfigurationException(e, "Unable to save settings into PwDatabase:\n" + e.Message);
            }

            try
            {
                SaveToLocal();
            }
            catch (Exception e)
            {
                throw new ConfigurationException(e, "Unable to save local config:" + _localFilename + "\n" + e.Message);
            }
        }

        private void ReadFromPwDatabase()
        {
            var pwEntry = GetConfPwEntry();
            if (pwEntry != null)
            {
                foreach (var prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var pwAttribute = prop.GetCustomAttributes(typeof(ConfStorageAttribute), false)
                        .Cast<ConfStorageAttribute>()
                        .FirstOrDefault(m => m.Type == ConfStorageType.PwDatabase);

                    if (pwAttribute == null)
                        continue;

                    try
                    {
                        var entryName = pwAttribute.FieldName ?? prop.Name;
                        if (prop.PropertyType == typeof(ProtectedString))
                        {
                            prop.SetValue(this, pwEntry.Strings.GetSafe(entryName), null);
                            continue;
                        }
                        if (prop.PropertyType == typeof(string))
                        {
                            prop.SetValue(this, pwEntry.Strings.GetSafe(entryName)?.ReadString(), null);
                            continue;
                        }

                        string svalue = pwEntry.Strings.GetSafe(entryName)?.ReadString() ?? "";
                        prop.SetValue(this, DeserializeObject(svalue, prop), null);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void SaveToPwDatabase()
        {
            if (_pwDatabase == null)
                throw new ConfigurationException("Unable to save setting. PwDatabase not setted");

            if (!_pwDatabase.IsOpen)
                throw new ConfigurationException("Unable to save setting. PwDatabase not opened");

            var pwEntry = GetConfPwEntry(_pwDatabase);

            if (pwEntry == null)
            {
                pwEntry = new PwEntry(true, true);
                pwEntry.Strings.Set( PwDefs.TitleField, new ProtectedString( false, "SafeVaultSync" ) );
                pwEntry.Strings.Set( PwDefs.NotesField, new ProtectedString( false, "" ) );

                _pwDatabase.RootGroup.AddEntry(pwEntry, true);
                _pwDatabase.RootGroup.Touch(true);
            }

            foreach (var prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var pwAttribute = prop.GetCustomAttributes(typeof(ConfStorageAttribute), false)
                    .Cast<ConfStorageAttribute>()
                    .FirstOrDefault(m => m.Type == ConfStorageType.PwDatabase);

                if (pwAttribute == null)
                    continue;

                var entryName = pwAttribute.FieldName ?? prop.Name;
                if (prop.PropertyType == typeof(ProtectedString))
                {
                    var newValue = (ProtectedString) prop.GetValue(this, null);
                    if (pwEntry.Strings.GetSafe(entryName)?.ReadString() != newValue?.ReadString())
                    {
                        pwEntry.Strings.Set(entryName, newValue);
                        pwEntry.Touch(true);
                    }
                    continue;
                }
                if (prop.PropertyType == typeof(string))
                {
                    var newValue = (string) prop.GetValue(this, null) ?? "";
                    if (pwEntry.Strings.GetSafe(entryName)?.ReadString() != newValue)
                    {
                        pwEntry.Strings.Set(entryName, new ProtectedString(pwAttribute.IsProtected, newValue));
                        pwEntry.Touch(true);
                    }
                    continue;
                }

                var newValue1 = SerializeObject(prop.GetValue(this, null), prop.PropertyType);
                if (pwEntry.Strings.GetSafe(entryName)?.ReadString() != newValue1)
                {
                    pwEntry.Strings.Set(entryName, new ProtectedString(pwAttribute.IsProtected, newValue1));
                    pwEntry.Touch(true);
                }
            }
        }

        private void ReadFromLocalConf()
        {
            if (File.Exists(_localFilename) == false)
                return;

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_localFilename);

            foreach (var prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = prop.GetCustomAttributes(typeof(ConfStorageAttribute), false)
                    .Cast<ConfStorageAttribute>()
                    .FirstOrDefault(m => m.Type == ConfStorageType.LocalConfig);

                if (attribute == null)
                    continue;

                var entryName = attribute.FieldName ?? prop.Name;
                var node = xdoc.SelectSingleNode("/VaultConfData/" + entryName);
                if (node == null)
                    continue;

                try
                {
                    if (prop.PropertyType == typeof(ProtectedString))
                    {
                        prop.SetValue(this, new ProtectedString(attribute.IsProtected, node.InnerText), null);
                        continue;
                    }
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, node.InnerText, null);
                        continue;
                    }

                    string svalue = node.InnerXml;
                    prop.SetValue(this, DeserializeObject(svalue, prop), null);
                }
                catch (Exception)
                {
                }
            }
        }

        private void CreateBackup(string location)
        {
            var folder = System.IO.Path.GetDirectoryName(location) + "\\backup";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            int ix = 0;
            while (true)
            {
                string bakName = folder + "\\" + string.Format("{0}.{1}.bak", Path.GetFileName(location), ix++);
                if (!File.Exists(bakName))
                {
                    File.Copy(location, bakName);
                    break;
                }
            }
        }

        private void SaveToLocal()
        {
            XmlDocument xdoc = new XmlDocument();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.AppendLine("<VaultConfData/>");
            xdoc.LoadXml(sb.ToString());

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var localConf = prop.GetCustomAttributes(typeof(ConfStorageAttribute), false)
                    .Cast<ConfStorageAttribute>()
                    .FirstOrDefault(m => m.Type == ConfStorageType.LocalConfig);

                if (localConf == null)
                    continue;

                var nodeName = localConf.FieldName ?? prop.Name;
                var node = xdoc.CreateElement(nodeName);
                xdoc.DocumentElement.AppendChild(node);

                if (prop.PropertyType == typeof(ProtectedString))
                {
                    node.InnerText = ((ProtectedString) prop.GetValue(this, null))?.ReadString() ?? "";
                    continue;
                }
                if (prop.PropertyType == typeof(string))
                {
                    node.InnerText = ((string) prop.GetValue(this, null)) ?? "";
                    continue;
                }
                node.InnerText =  SerializeObject(prop.GetValue(this, null), prop.PropertyType);
            }


            SaveXmlDoc(xdoc);
        }

        private void SaveXmlDoc(XmlDocument xdoc)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.CloseOutput = true;
            xws.Encoding = Encoding.UTF8;
            xws.Indent = true;
            xws.IndentChars = "\t";

            string content;
            using (var ms = new MemoryStream())
            using (var output = new StreamWriter(ms, Encoding.UTF8))
            {
                XmlWriter xw = XmlWriter.Create(output, xws);
                xdoc.Save(xw);
                ms.Flush();

                ms.Position = 0;
                using (var read = new StreamReader(ms, Encoding.UTF8))
                {
                    content = read.ReadToEnd();
                }
            }
            if (File.Exists(_localFilename))
            {
                var content2 = File.ReadAllText(_localFilename);
                if (content == content2)
                    return;

                CreateBackup(_localFilename);
            }
            File.WriteAllText(_localFilename, content);
        }

        public PwEntry GetConfPwEntry()
        {
            return GetConfPwEntry(_pwDatabase);
        }

        public static PwEntry GetConfPwEntry(PwDatabase pwDatabase)
        {
            var sp = new SearchParameters
            {
                SearchString = "SafeVaultSync",
                ComparisonMode = StringComparison.OrdinalIgnoreCase,
                RespectEntrySearchingDisabled = false,
                SearchInGroupNames = false,
                SearchInNotes = false,
                SearchInOther = false,
                SearchInPasswords = false,
                SearchInTags = false,
                SearchInTitles = true,
                SearchInUrls = true,
                SearchInUserNames = false,
                SearchInUuids = false
            };

            PwObjectList<PwEntry> accounts = new PwObjectList<PwEntry>();
            pwDatabase.RootGroup.SearchEntries(sp, accounts);
            return (accounts.UCount >= 1)
                ? accounts.GetAt(0)
                : null;
        }

        private static string SerializeObject(object value, Type type)
        {
            if (value == null)
                return "";

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.CloseOutput = true;
            xws.Encoding = StrUtil.Utf8;
            xws.Indent = true;
            xws.IndentChars = "\t";
            xws.OmitXmlDeclaration = true;

            using (var ms = new MemoryStream())
            using (var output = new StreamWriter(ms, Encoding.UTF8))
            {
                XmlWriter xw = XmlWriter.Create(output, xws);
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(xw, value);

                ms.Flush();
                ms.Position = 0;
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(ms);
                return xdoc.DocumentElement?.InnerXml;
            }
        }

        private static object DeserializeObject(string svalue, PropertyInfo prop)
        {
            object value = null;
            if (!string.IsNullOrEmpty(svalue))
            {
                var xdoc = string.Format("<{0}>{1}</{0}>", prop.PropertyType.Name, svalue);

                using (var textReader = new StringReader(xdoc))
                {
                    XmlSerializer xs = new XmlSerializer(prop.PropertyType);
                    value = xs.Deserialize(textReader);
                }
            }
            return value;
        }
    }
}
