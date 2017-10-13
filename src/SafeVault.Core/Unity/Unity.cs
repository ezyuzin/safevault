using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SafeVault.Logger;

namespace SafeVault.Unity
{
    public class Unity : IUnity
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<Type, object> _instance;
        private readonly Dictionary<Type, Type> _registrations;

        public Unity()
        {
            _instance = new Dictionary<Type, object>();
            _registrations = new Dictionary<Type, Type>();

            _registrations.Add(typeof(IUnity), this.GetType());
            _instance.Add(typeof(IUnity), this);
        }

        public void RegisterInstance<T, T1>(T1 obj) where T1 : T
        {
            _instance.Add(typeof(T), obj);
            _registrations.Add(typeof(T), typeof(T1));
        }

        public void Register<T, T1>() where T1 : T
        {
            _registrations.Add(typeof(T), typeof(T1));
        }

        public void Validate()
        {
            foreach (var reg in _registrations)
            {
                Logger.Debug($"{reg.Key.Name} [{reg.Value.Name}]");
                var types = new List<Type>();
                Validate(reg.Key, 1, types);
            }
        }
        public void Validate(Type type1, int offset, List<Type> validated)
        {
            foreach (var prop in _registrations[type1].GetProperties())
            {
                bool hasAttributes = prop.GetCustomAttributes(typeof(DependencyAttribute), false).Any();
                if (!hasAttributes)
                    continue;

                var type = prop.PropertyType;

                if (!_registrations.ContainsKey(type))
                {
                    Logger.Error($"--> {type.Name}".PadLeft(offset * 4));
                    throw new UnityException($"Type {type.FullName} is not registered in container");
                }

                var rtype = _registrations[type];
                if (validated.Contains(type))
                {
                    Logger.Error($"--> {type.Name} [{rtype.Name}]".PadLeft(offset * 4));
                    throw new UnityException($"Type {type.FullName} has cycled references");
                }

                Logger.Debug($"--> {type.Name} [{rtype.Name}]");
            }
        }


        public object BuildUp(Type type, object obj)
        {
            foreach (var prop in type.GetProperties())
            {
                bool hasAttributes = prop.GetCustomAttributes(typeof(DependencyAttribute), false).Any();
                if (!hasAttributes)
                    continue;

                var dependency = Resolve(prop.PropertyType);
                #if NETSTANDARD2_0
                prop.SetValue(obj, dependency);
                #endif

                #if NETFX
                prop.SetValue(obj, dependency, null);
                #endif
            }
            return obj;
        }

        public T Resolve<T>()
        {
            return (T) Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (_instance.ContainsKey(type))
                return _instance[type];

            if (!_registrations.ContainsKey(type))
                throw new UnityException($"Type {type.FullName} is not registered in container");

            object instance = null;
            try
            {
                var type1 = _registrations[type];


                var constuctor = type1.GetConstructors()
                    .Where(m =>
                    {
                        return m.GetParameters()
                            .All(param =>
                            {
                                var pType = param.ParameterType;
                                return (_registrations.ContainsKey(pType) || _instance.ContainsKey(pType));
                            });
                    })
                    .OrderByDescending(m => m.GetParameters().Length)
                    .FirstOrDefault();

                if (constuctor == null)
                    throw new UnityException($"Unable to find appropriate contructor for type {type1.FullName}");

                var args = constuctor.GetParameters().Select(m => Resolve(m.ParameterType)).ToArray();

                try
                {
                    instance = Activator.CreateInstance(type1, BindingFlags.CreateInstance, null, args,
                        CultureInfo.CurrentCulture);
                }
                catch (Exception e)
                {
                    throw new UnityException(e, $"Unable create instance of {type1.FullName}");
                }

                BuildUp(type1, instance);
                _instance.Add(type, instance);
            }
            catch
            {
                (instance as IDisposable)?.Dispose();
                throw;
            }
            return instance;
        }

        public void Dispose()
        {
            foreach (var instance in _instance.Values)
            {
                if (instance == this)
                    continue;

                (instance as IDisposable)?.Dispose();
            }
        }
    }
}