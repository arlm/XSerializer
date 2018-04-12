using System;
using System.Linq.Expressions;
using System.Reflection;

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

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parameter1 = Expression.Parameter(typeof(object), p1.Name);
            var parameter2 = Expression.Parameter(typeof(object), p2.Name);

            Expression body = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                Expression.Convert(parameter1, p1.ParameterType),
                Expression.Convert(parameter2, p2.ParameterType));

            var lambda =
                Expression.Lambda<Action<object, object, object>>(body, instanceParameter, parameter1, parameter2);

            return lambda.Compile();
        }
    }
}