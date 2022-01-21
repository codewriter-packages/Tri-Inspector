using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TriInspector.Utilities
{
    internal static class TriReflectionCompileExtensions
    {
        public static Func<object, TResult> CompileInstanceProperty<TResult>(this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = GetProperty(type, name, flags);

            var target = Expression.Parameter(typeof(object));
            var body = Expression.Call(Expression.Convert(target, type), property.GetMethod);
            var lambda = Expression.Lambda<Func<object, TResult>>(body, target);
            return lambda.Compile();
        }

        public static Action<object> CompileVoidInstanceMethod(this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 0, flags);

            var target = Expression.Parameter(typeof(object));
            var body = Expression.Call(Expression.Convert(target, type), method);
            var lambda = Expression.Lambda<Action<object>>(body, target);
            return lambda.Compile();
        }

        public static Action<object, T1> CompileVoidInstanceMethod<T1>(this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 1, flags);

            var target = Expression.Parameter(typeof(object));
            var a1 = Expression.Parameter(typeof(T1));
            var body = Expression.Call(Expression.Convert(target, type), method, a1);
            var lambda = Expression.Lambda<Action<object, T1>>(body, target, a1);
            return lambda.Compile();
        }

        public static Func<object, T1, T2, T3, TResult> CompileInstanceMethod<T1, T2, T3, TResult>(
            this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 3, flags);

            var target = Expression.Parameter(typeof(object));
            var a1 = Expression.Parameter(typeof(T1));
            var a2 = Expression.Parameter(typeof(T2));
            var a3 = Expression.Parameter(typeof(T3));
            var body = Expression.Call(Expression.Convert(target, type), method, a1, a2, a3);
            var lambda = Expression.Lambda<Func<object, T1, T2, T3, TResult>>(body, target, a1, a2, a3);
            return lambda.Compile();
        }

        public static Func<object, T1, T2, T3, T4, TResult> CompileInstanceMethod<T1, T2, T3, T4, TResult>(
            this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 4, flags);

            var target = Expression.Parameter(typeof(object));
            var a1 = Expression.Parameter(typeof(T1));
            var a2 = Expression.Parameter(typeof(T2));
            var a3 = Expression.Parameter(typeof(T3));
            var a4 = Expression.Parameter(typeof(T4));
            var body = Expression.Call(Expression.Convert(target, type), method, a1, a2, a3, a4);
            var lambda = Expression.Lambda<Func<object, T1, T2, T3, T4, TResult>>(body, target, a1, a2, a3, a4);
            return lambda.Compile();
        }

        public static Func<T1, TResult> CompileStaticMethod<T1, TResult>(
            this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 1, flags);

            var a1 = Expression.Parameter(typeof(T1));
            var body = Expression.Call(method, a1);
            var lambda = Expression.Lambda<Func<T1, TResult>>(body, a1);
            return lambda.Compile();
        }

        public static Action<T1, T2> CompileStaticVoidMethod<T1, T2>(
            this Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var method = GetMethod(type, name, 2, flags);

            var a1 = Expression.Parameter(typeof(T1));
            var a2 = Expression.Parameter(typeof(T2));
            var body = Expression.Call(method, a1, a2);
            var lambda = Expression.Lambda<Action<T1, T2>>(body, a1, a2);
            return lambda.Compile();
        }

        private static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            var property = type.GetProperties(flags).SingleOrDefault(it => it.Name == name);
            return property ?? throw new InvalidOperationException($"Property {name} of type {type} not found");
        }

        private static MethodInfo GetMethod(Type type, string name, int parametersCount, BindingFlags flags)
        {
            var method = type.GetMethods(flags)
                .SingleOrDefault(it => it.Name == name && it.GetParameters().Length == parametersCount);

            return method ?? throw new InvalidOperationException(
                $"Method {name} of type {type} with {parametersCount} args not found");
        }
    }
}