using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;

namespace CodeEndeavors.ServiceHost.Extensions
{
    public static class ReflectionExtensions
    {
        public static object InvokeStaticMethod(this Type o, string name)
        {
            return ReflectionExtensions.InvokeStaticMethod(o, name, null);
        }

        public static object InvokeStaticMethod(this Type o, string name, params object[] args)
        {
            return o.InvokeMember(name, BindingFlags.InvokeMethod, null, null, args);
        }

        public static T InvokeStaticMethod<T>(this Type o, string name, params object[] args)
        {
            return (T)((object)o.InvokeMember(name, BindingFlags.InvokeMethod, null, null, args));
        }

        public static object InvokePropertyGet(this object o, string name, params object[] args)
        {
            return o.GetType().InvokeMember(name, BindingFlags.GetProperty, null, RuntimeHelpers.GetObjectValue(o), args);
        }

        public static T InvokePropertyGet<T>(this object o, string name)
        {
            return (T)((object)ReflectionExtensions.InvokePropertyGet(RuntimeHelpers.GetObjectValue(o), name));
        }

        public static T InvokePropertyGet<TType, T>(this TType o, string name)
        {
            return (T)((object)ReflectionExtensions.InvokePropertyGet(o, name));
        }

        public static object InvokePropertyGet(this object o, string name)
        {
            return ReflectionExtensions.InvokePropertyGet(RuntimeHelpers.GetObjectValue(o), name, null);
        }

        public static void InvokePropertySet(this object o, string name, params object[] args)
        {
	        bool flag = args.Length == 1;
	        if (flag)
	        {
		        o.GetType().InvokeMember(name, BindingFlags.SetProperty, null, RuntimeHelpers.GetObjectValue(o), args);
	        }
	        else
	        {
		        o.GetType().GetProperty(name).SetValue(RuntimeHelpers.GetObjectValue(o), RuntimeHelpers.GetObjectValue(args[1]), new object[]
		        {
			        RuntimeHelpers.GetObjectValue(args[0])
		        });
	        }
        }

        public static void InvokeStaticPropertySet(this Type o, string name, params object[] args)
        {
            bool flag = args.Length == 1;
            if (flag)
            {
                o.InvokeMember(name, BindingFlags.Static | BindingFlags.SetProperty, null, null, args);
            }
            else
            {
                ReflectionExtensions.InvokePropertySet(RuntimeHelpers.GetObjectValue(o.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty).GetValue(null, null)), "Item", new object[]
		        {
			        RuntimeHelpers.GetObjectValue(args[0]),
			        RuntimeHelpers.GetObjectValue(args[1])
		        });
            }
        }

        public static Type ToType(this string s)
        {
            return ToType(s, true);
        }

        public static Type ToType(this string s, bool throwError)
        {
            return BuildManager.GetType(s, throwError, true);
        }

        public static Type ToType(this string s, bool onlyPublic, bool throwError)
        {

            var assemblies = System.Threading.Thread.GetDomain().GetAssemblies();
            foreach (var a in assemblies)
            {
                var t = a.GetType(s, false, true);
                if (t != null)
                {
                    if (onlyPublic)
                    {
                        if (t.IsPublic)
                            return t;
                    }
                    else
                        return t;
                }
            }
            return null;
        }

        public static object ReflectToObject(this string s)
        {
            return ReflectionExtensions.ReflectToObject(ReflectionExtensions.ToType(s));
        }
        public static object ReflectToObject(this Type t)
        {
            return Activator.CreateInstance(t);
        }
        public static object ReflectToObject(this string s, params object[] args)
        {
            return Activator.CreateInstance(ReflectionExtensions.ToType(s), args);
        }
        public static object ReflectToObject(this Type t, params object[] args)
        {
            return Activator.CreateInstance(t, args);
        }
    }
}
