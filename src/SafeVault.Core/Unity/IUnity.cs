using System;

namespace SafeVault.Unity
{
    public interface IUnity : IDisposable
    {
        void Register<T, T1>() where T1 : T;
        object BuildUp(Type type, object obj);
        T Resolve<T>();
        object Resolve(Type type);
        void RegisterInstance<T, T1>(T1 obj) where T1 : T;
    }
}