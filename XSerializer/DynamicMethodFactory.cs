using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace XSerializer
{
    public static class DynamicMethodFactory
    {
        public static Func<T> CreateDefaultConstructorFunc<T>(this Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new ArgumentException("Type argument must have a default constructor in order to create a constructor func.");
            }

            var dm = new DynamicMethod(string.Empty, typeof(T), Type.EmptyTypes, type);
            var il = dm.GetILGenerator();

            il.DeclareLocal(type);
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var func = (Func<T>)dm.CreateDelegate(typeof(Func<T>));
            return func;
        }

        public static Func<object, T> CreateFunc<T>(MethodInfo method)
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

        public static Func<object, object> CreateGetPropertyValueFunc(Type containerType, string propName)
        {
            var param = Expression.Parameter(typeof(object));
            var func = Expression.Lambda(
                Expression.Convert(
                    Expression.PropertyOrField(
                        Expression.Convert(
                            param,
                            containerType),
                        propName),
                    typeof(object)),
                param);

            return (Func<object, object>)func.Compile();
        }

        public static Action<object, object> CreateAction(MethodInfo method)
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

        public static Action<object, object, object> CreateTwoArgAction(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var p1 = parameters[0];
            var p2 = parameters[1];

            var dynamicMethod = new DynamicMethod(
                method.Name + "_Invoker",
                typeof(void),
                new[] { typeof(object), typeof(object), typeof(object) },
                typeof(DynamicMethodFactory));

            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg, 0);

            il.Emit(OpCodes.Ldarg, 1);
            if (p1.ParameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, p1.ParameterType);
            }

            il.Emit(OpCodes.Ldarg, 2);
            if (p2.ParameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, p2.ParameterType);
            }

            il.EmitCall(OpCodes.Callvirt, method, null);
            il.Emit(OpCodes.Ret);

            return (Action<object, object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object, object>));
        }
    }
}