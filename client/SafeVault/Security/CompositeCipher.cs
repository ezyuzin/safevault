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
using SafeVault.Misc;

namespace SafeVault.Security
{
    public class CompositeCipher : ICipher
    {
        private readonly Lazy<ICipher>[] _ciphers;

        public CompositeCipher(params Lazy<ICipher>[] ciphers)
        {
            _ciphers = ciphers;
        }

        public byte[] Decrypt(byte[] data)
        {
            for (int i = _ciphers.Length - 1; i >=0; i--)
                data = _ciphers[i].Value.Decrypt(data);

            return data;
        }

        public byte[] Encrypt(byte[] data)
        {
            for (int i = 0; i < _ciphers.Length; i++)
                data = _ciphers[i].Value.Encrypt(data);

            return data;
        }

        public void Dispose()
        {
        }
    }
}