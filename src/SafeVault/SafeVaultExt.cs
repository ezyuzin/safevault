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
using System.Reflection;
using KeePass.Plugins;

namespace SafeVault
{
    public sealed class SafeVaultExt : Plugin
    {
        private static string _productVersion;

        private static IPluginHost _host = null;
        private SafeVaultKeyProvider _prov = null;
        private SafeVaultSync _sync;


        public static IPluginHost Host
        {
            get { return _host; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
                return false;

            _host = host;

            _prov = new SafeVaultKeyProvider();
            _host.KeyProviderPool.Add(_prov);

            _sync = new SafeVaultSync();
            _sync.Initialize(host);

            return true;
        }

        public override void Terminate()
        {
            if (_host == null)
                return;

            if (_sync != null)
                _sync.Terminate();

            _host.KeyProviderPool.Remove(_prov);
            _sync = null;
            _prov = null;
            _host = null;
        }

        public static string VersionString()
        {
            if (_productVersion == null)
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                _productVersion = "v" + version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
            }
            return _productVersion;
        }
    }
}
