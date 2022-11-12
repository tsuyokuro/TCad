using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Reflection.Emit;

using OpenTK.Platform;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using TCad;
using System.Security.Policy;

namespace OpenGL.GLU;

public static partial class Glu
{
    private const string Library = "glu32.dll";

    private static Dictionary<string, bool> AvailableExtensions = new Dictionary<string, bool>();
    private static bool rebuildExtensionList = true;

    private static Type importsClass = typeof(Imports);

    private static IntPtr HandleGluLibrary;

    public static void Initialize()
    {
        HandleGluLibrary = WinAPI.LoadLibrary(Library);
        LoadAll();
    }

    public static void Dispose()
    {
        WinAPI.FreeLibrary(HandleGluLibrary);
    }

    public static void LoadAll()
    {
        int supported = 0;
        Type extensions_class = typeof(Glu).GetNestedType("Delegates", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (extensions_class == null)
        {
            throw new InvalidOperationException("The specified type does not have any loadable extensions.");
        }

        FieldInfo[] delegates = extensions_class.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (delegates == null)
        {
            throw new InvalidOperationException("The specified type does not have any loadable extensions.");
        }

        foreach (FieldInfo f in delegates)
        {
            Delegate d = LoadDelegate(f.Name, f.FieldType);
            if (d != null)
                ++supported;

            f.SetValue(null, d);
        }

        rebuildExtensionList = true;
    }

    public static bool Load(string function)
    {
        // Glu does not contain any extensions - this method does nothing.
        return true;
    }

    private static Delegate LoadDelegate(string name, Type signature)
    {
        Delegate ret = GetExtensionDelegate(name, signature);
        if (ret != null)
        {
            return ret;
        }

        MethodInfo m = importsClass.GetMethod(name.Substring(3), BindingFlags.Static | BindingFlags.NonPublic);
        if (m == null) {
            return null;
        }

        return Delegate.CreateDelegate(signature, m);
    }

    private static Delegate GetExtensionDelegate(string name, Type signature)
    {
        IntPtr address = GetAddress(name);

        if (address == IntPtr.Zero ||
            address == new IntPtr(1) ||     // Workaround for buggy nvidia drivers which return
            address == new IntPtr(2))       // 1 or 2 instead of IntPtr.Zero for some extensions.
        {
            return null;
        }
        else
        {
            return Marshal.GetDelegateForFunctionPointer(address, signature);
        }
    }

    private static IntPtr GetAddress(string function)
    {
        return WinAPI.GetProcAddress(HandleGluLibrary, function);
    }

    public static bool SupportsExtension(string name)
    {
        if (rebuildExtensionList)
        {
            BuildExtensionList();
        }

        // Search the cache for the string. Note that the cache substitutes
        // strings "1.0" to "2.1" with "GL_VERSION_1_0" to "GL_VERSION_2_1"
        if (AvailableExtensions.ContainsKey(name))
        {
            return AvailableExtensions[name];
        }

        return false;
    }

    private static void BuildExtensionList()
    {
        // Assumes there is an opengl context current.

        AvailableExtensions.Clear();

        string version_string = Glu.GetString(GluStringName.Version);
        if (String.IsNullOrEmpty(version_string))
        {
            throw new ApplicationException("Failed to build extension list. Is there an opengl context current?");
        }

        string version = version_string.Trim(' ');
        if (version.StartsWith("1.0"))
        {
            AvailableExtensions.Add("VERSION_1_0", true);
        }
        else if (version.StartsWith("1.1"))
        {
            AvailableExtensions.Add("VERSION_1_0", true);
            AvailableExtensions.Add("VERSION_1_1", true);
        }
        else if (version.StartsWith("1.2"))
        {
            AvailableExtensions.Add("VERSION_1_0", true);
            AvailableExtensions.Add("VERSION_1_1", true);
            AvailableExtensions.Add("VERSION_1_2", true);
        }
        else if (version.StartsWith("1.3"))
        {
            AvailableExtensions.Add("VERSION_1_0", true);
            AvailableExtensions.Add("VERSION_1_1", true);
            AvailableExtensions.Add("VERSION_1_2", true);
            AvailableExtensions.Add("VERSION_1_3", true);
        }

        string extension_string = Glu.GetString(GluStringName.Extensions);
        if (string.IsNullOrEmpty(extension_string))
        {   // no extensions are available
            return;
        }

        string[] extensions = extension_string.Split(' ');
        foreach (string ext in extensions)
        {
            AvailableExtensions.Add(ext, true);
        }

        rebuildExtensionList = false;
    }
}

#if false

//public delegate object

public delegate void FastVoidInvokeHandler(object target, object[] paramters);
public delegate object FastInvokeHandler(object target, object[] paramters);
public static class FastInvoker
{
    /// <summary>
    /// Use this one instead of MethodInfo.Invoke, this way it is 50 times quicker.
    /// 
    /// <example>
    /// string Filter = "FirstName = 'Ton'"
    /// MethodInfo mi = typeof(Person).GetMethod("GetAll");
    /// snoei.net.Reflection.FastInvoker.FastInvokeHandler fi = snoei.net.Reflection.FastInvoker.GetMethodInvoker( mi );
    //    return fi.Invoke( Person, new object[]{Filter} );
    /// //Calls Person.GetAll(string Filter);
    /// </example>
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    public static Delegate GetMethodInvoker(MethodInfo methodInfo)
    {
        DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, methodInfo.ReturnType, new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
        ILGenerator il = dynamicMethod.GetILGenerator();
        ParameterInfo[] ps = methodInfo.GetParameters();
        Type[] paramTypes = new Type[ps.Length];

        for (int i = 0; i < paramTypes.Length; i++)
        {
            if (ps[i].ParameterType.IsByRef)
                paramTypes[i] = ps[i].ParameterType.GetElementType();
            else
                paramTypes[i] = ps[i].ParameterType;
        }

        LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

        for (int i = 0; i < paramTypes.Length; i++)
            locals[i] = il.DeclareLocal(paramTypes[i], true);

        for (int i = 0; i < paramTypes.Length; i++)
        {
            il.Emit(OpCodes.Ldarg_1);
            EmitFastInt(il, i);
            il.Emit(OpCodes.Ldelem_Ref);
            EmitCastToReference(il, paramTypes[i]);
            il.Emit(OpCodes.Stloc, locals[i]);
        }

        if (!methodInfo.IsStatic)
            il.Emit(OpCodes.Ldarg_0);

        for (int i = 0; i < paramTypes.Length; i++)
        {
            if (ps[i].ParameterType.IsByRef)
                il.Emit(OpCodes.Ldloca_S, locals[i]);
            else
                il.Emit(OpCodes.Ldloc, locals[i]);
        }

        if (methodInfo.IsStatic)
            il.EmitCall(OpCodes.Call, methodInfo, null);
        else
            il.EmitCall(OpCodes.Callvirt, methodInfo, null);

        if (methodInfo.ReturnType == typeof(void))
            il.Emit(OpCodes.Ldnull);
        else
            EmitBoxIfNeeded(il, methodInfo.ReturnType);

        for (int i = 0; i < paramTypes.Length; i++)
        {
            if (ps[i].ParameterType.IsByRef)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldloc, locals[i]);
                if (locals[i].LocalType.IsValueType)
                    il.Emit(OpCodes.Box, locals[i].LocalType);
                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        il.Emit(OpCodes.Ret);

        if (methodInfo.ReturnType == typeof(void))
            return dynamicMethod.CreateDelegate(typeof(FastVoidInvokeHandler));
        else
            return dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
    }

    private static void EmitCastToReference(ILGenerator il, System.Type type)
    {
        if (type.IsValueType)
            il.Emit(OpCodes.Unbox_Any, type);
        else
            il.Emit(OpCodes.Castclass, type);
    }

    private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
    {
        if (type.IsValueType)
            il.Emit(OpCodes.Box, type);
    }

    private static void EmitFastInt(ILGenerator il, int value)
    {
        switch (value)
        {
            case -1:
                il.Emit(OpCodes.Ldc_I4_M1);
                return;
            case 0:
                il.Emit(OpCodes.Ldc_I4_0);
                return;
            case 1:
                il.Emit(OpCodes.Ldc_I4_1);
                return;
            case 2:
                il.Emit(OpCodes.Ldc_I4_2);
                return;
            case 3:
                il.Emit(OpCodes.Ldc_I4_3);
                return;
            case 4:
                il.Emit(OpCodes.Ldc_I4_4);
                return;
            case 5:
                il.Emit(OpCodes.Ldc_I4_5);
                return;
            case 6:
                il.Emit(OpCodes.Ldc_I4_6);
                return;
            case 7:
                il.Emit(OpCodes.Ldc_I4_7);
                return;
            case 8:
                il.Emit(OpCodes.Ldc_I4_8);
                return;
        }

        if (value > -129 && value < 128)
            il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
        else
            il.Emit(OpCodes.Ldc_I4, value);
    }
}

#endif
