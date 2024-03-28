using System;
using System.Reflection;
using JetBrains.Annotations;
using NUnit.Framework;

public static class TestUtils
{
    public static T ConstructPrivate<T>(params object[] args)
    {
        return (T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
    }

    public static void InvokePrivateMethod<T>(T instance, string methodName, params object[] parameters)
    {
        typeof(T).InvokeMember(methodName,
            BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, instance, parameters);
    }
    public static TReturned InvokePrivateMethod<T, TReturned>(T instance, string methodName, params object[] parameters)
    {
        return (TReturned)typeof(T).InvokeMember(methodName,
            BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, instance, parameters);
    }

    public static void InvokePrivateStaticMethod<T>(string methodName, params object[] parameters)
    {
        typeof(T).InvokeMember(methodName,
            BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, parameters);
    }

    public static TReturned InvokePrivateStaticMethod<T, TReturned>(string methodName, params object[] parameters)
    {
        return (TReturned)typeof(T).InvokeMember(methodName,
            BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, parameters);
    }
}

public class TestUtilsTests
{
    [Test]
    public void CanConstructPrivateClass()
    {
        ClassWithPrivateConstructorAndMember instance = TestUtils.ConstructPrivate<ClassWithPrivateConstructorAndMember>();
        Assert.IsNotNull(instance);
    }

    [Test]
    public void CanCallPrivateMethod()
    {
        ClassWithPrivateConstructorAndMember instance = TestUtils.ConstructPrivate<ClassWithPrivateConstructorAndMember>();
        bool result = TestUtils.InvokePrivateMethod<ClassWithPrivateConstructorAndMember, bool>(instance, "PrivateMethod");
        Assert.IsTrue(result);
    }

    [Test]
    public void CanCallPrivateStaticMethod()
    {
        bool result = TestUtils.InvokePrivateStaticMethod<ClassWithPrivateConstructorAndMember, bool>("PrivateStaticMethod");
        Assert.IsTrue(result);
    }
}

[UsedImplicitly]
public class ClassWithPrivateConstructorAndMember
{
    private ClassWithPrivateConstructorAndMember()
    {
    }

    [UsedImplicitly]
    private bool PrivateMethod()
    {
        return true;
    }

    [UsedImplicitly]
    private static bool PrivateStaticMethod()
    {
        return true;
    }
}