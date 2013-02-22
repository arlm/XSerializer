using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XSerializer
{
    public static class DynamicMethodFactory
    {
        public static Func<object, T> CreateGetMethod<T>(MethodInfo method)
        {
            var dynamicMethod = new DynamicMethod(
                method.Name + "_Invoker",
                typeof(T),
                new[] { typeof(object) },
                typeof(DynamicMethodFactory));

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg, 0);
            il.EmitCall(OpCodes.Callvirt, method, null);

            if (method.ReturnType.IsValueType && typeof(T) == typeof(object))
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return (Func<object, T>)dynamicMethod.CreateDelegate(typeof(Func<object, T>));
        }

        public static Action<object, object> CreateSetMethod(MethodInfo method)
        {
            var parameter = method.GetParameters()[0];

            var dynamicMethod = new DynamicMethod(
                method.Name + "_Invoker",
                typeof(void),
                new[] { typeof(object), typeof(object) },
                typeof(DynamicMethodFactory));

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg, 0);
            il.Emit(OpCodes.Ldarg, 1);

            if (parameter.ParameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, parameter.ParameterType);
            }

            il.EmitCall(OpCodes.Callvirt, method, null);
            il.Emit(OpCodes.Ret);

            return (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
        }
    }
}