/*
  SafeVaultKeyProvider Plugin
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
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace SafeVaultKeyPlugin
{
    public sealed class VaultConf
    {
        private const string CONFFILE_EXTENSION = ".vault.xml";

        public VaultConfData Data { get; private set; }
        private IOConnectionInfo _ioc;

        public string GetDisplayName()
        {
            return _ioc.GetDisplayName();
        }

        public bool IsLoaded { get; private set; }


        public VaultConf(IOConnectionInfo ioc)
        {
            _ioc = ioc.CloneDeep();
            _ioc.Path = UrlUtil.StripExtension(_ioc.Path) + CONFFILE_EXTENSION;

            Stream stream = null;
            Data = new VaultConfData();
            try
            {
                stream = IOConnection.OpenRead(_ioc);
                XmlSerializer xs = new XmlSerializer(typeof(VaultConfData));
                Data = (VaultConfData)xs.Deserialize(stream);
                IsLoaded = true;
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public bool Save()
        {
            Stream stream = null;

            try
            {
                if (File.Exists(_ioc.Path))
                {
                    File.Copy(_ioc.Path, _ioc.Path + ".bak", true);
                }

                stream = IOConnection.OpenWrite(_ioc);

                XmlWriterSettings xws = new XmlWriterSettings();
                xws.CloseOutput = true;
                xws.Encoding = StrUtil.Utf8;
                xws.Indent = true;
                xws.IndentChars = "\t";

                XmlWriter xw = XmlWriter.Create(stream, xws);
                XmlSerializer xs = new XmlSerializer(typeof(VaultConfData));
                xs.Serialize(xw, this.Data);
                xw.Close();
                return true;
            }
            catch (Exception) { Debug.Assert(false); }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }
    }
}
