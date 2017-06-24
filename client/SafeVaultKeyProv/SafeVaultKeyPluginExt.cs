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

using KeePass.Plugins;
using SafeVaultKeyPlugin;

namespace SafeVaultKeyProv
{
    public sealed class SafeVaultKeyProvExt : Plugin
    {
        private static IPluginHost _host = null;
        private static VaultKeyProvider _prov = null;

        public const string SHORT_PRODUCT_NAME = "SafeVault Key Provider";
        public const string PRODUCT_NAME = "SafeVault Key Plugin";

        public static IPluginHost Host
        {
            get { return _host; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
                return false;

            _host = host;

            _prov = new VaultKeyProvider();
            _host.KeyProviderPool.Add(_prov);
            return true;
        }

        public override void Terminate()
        {
            if (_host == null)
                return;

            _host.KeyProviderPool.Remove(_prov);
            _prov = null;
            _host = null;
        }
    }
}
