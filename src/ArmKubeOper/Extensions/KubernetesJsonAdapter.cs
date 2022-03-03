using System;
using System.Reflection;

namespace ArmKubeOper.Extensions
{

    /// <summary>
    /// Provides some internal JSON methods from the Kubernetes client API.
    /// </summary>
    public static class KubernetesJsonAdapter
    {

        static readonly Type KubernetesJsonType = typeof(k8s.GenericClient).Assembly.GetType("k8s.KubernetesJson");
        static readonly MethodInfo DeserializeMethod = KubernetesJsonType.GetMethod("Deserialize", 1, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);

        /// <summary>
        /// Deserializes the given JSON using the KubernetesJson class from the k8s client.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static TValue Deserialize<TValue>(string json) => (TValue)DeserializeMethod.MakeGenericMethod(typeof(TValue)).Invoke(null, new object[] { json });

    }

}
