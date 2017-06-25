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

namespace SafeVault.Misc
{
    public class Lazy<T>
    {
        private readonly object _lock = new object();
        private readonly Func<T> _createCallback;
        private volatile bool _hasValue;
        private T _value;

        public bool HasValue {get { return _hasValue; } }
        public T Value
        {
            get
            {
                if (_hasValue)
                    return _value;

                lock (_lock)
                {
                    if (!_hasValue)
                    {
                        _value = _createCallback();
                        _hasValue = true;
                    }
                }
                return _value;
            }
        }

        public Lazy(Func<T> createCallback)
        {
            _createCallback = createCallback;
            _hasValue = false;
        }
    }
}