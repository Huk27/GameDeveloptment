﻿using System.Reflection;
using System.Reflection.Emit;
using Nanoray.Pintail;

namespace AtraCore.Framework.ReflectionManager;

/// <summary>
/// Handles using pintail for reflection stuff.
/// </summary>
internal static class PintailReflection
{
    private static readonly Lazy<ProxyManager<Nothing>> proxyManager = new(() =>
    {
        AssemblyBuilder? assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"AtraCore.Proxies, Version={typeof(PintailReflection).Assembly.GetName().Version}, Culture=neutral"),
            AssemblyBuilderAccess.Run); // Maybe I want RunAndCollect?

        ModuleBuilder? moduleBuilder = assemblyBuilder.DefineDynamicModule("Proxies");
        return new ProxyManager<Nothing>(
            moduleBuilder,
            new(noMatchingMethodHandler: ProxyManagerConfiguration<Nothing>.ThrowExceptionNoMatchingMethodHandler,
                enumMappingBehavior: ProxyManagerEnumMappingBehavior.Allow));
    });

    private static ProxyManager<Nothing> ProxyManager => proxyManager.Value;

    internal static TInterface GetProxy<TInterface>(object provider)
        where TInterface : class
    {
        return ProxyManager.ObtainProxy<TInterface>(provider);
    }

    internal static bool TryProxy<TInterface>(object provider, out TInterface? proxy)
        where TInterface : class
    {
        return ProxyManager.TryProxy(provider, out proxy);
    }
}
