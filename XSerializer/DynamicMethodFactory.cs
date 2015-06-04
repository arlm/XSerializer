using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace XSerializer
{
    internal static class DynamicMethodFactory
    {
        public static Func<T> CreateDefaultConstructorFunc<T>(this Type type)
        {
            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new ArgumentException("Type argument must have a default constructor in order to create a constructor func.");
            }

            var expression = Expression.Lambda<Func<T>>(Expression.New(ctor));
            var func = expression.Compile();
            return func;
        }

        public static Func<object, T> CreateFunc<T>(MethodInfo method)
        {
            var parameter = Expression.Parameter(typeof(object));

            UnaryExpression instanceCast =
                method.DeclaringType.IsValueType
                ? Expression.Convert(parameter, method.DeclaringType)
                : Expression.TypeAs(parameter, method.DeclaringType);

            var call = 
                Expression.Call(
                    instanceCast,
                    method);

            Expression body =
                typeof(T).IsValueType
                ? (Expression)call
                : Expression.TypeAs(call, typeof(T));

            var expression = Expression.Lambda<Func<object, T>>(
                body,
                parameter);

            return expression.Compile();
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
            var methodParameter = method.GetParameters()[0];

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var lambda = Expression.Lambda<Action<object, object>>(
                Expression.Call(
                    Expression.Convert(instanceParameter, method.DeclaringType),
                    method,
                    Expression.Convert(valueParameter, methodParameter.ParameterType)),
                new[] { instanceParameter, valueParameter });

            return lambda.Compile();
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