using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace XSerializer
{
    public class PropertyInvoker
    {
        public PropertyInvoker(PropertyInfo propertyInfo)
        {
            GetValue = (Func<object, object>)CreateDynamicMethod(propertyInfo.GetGetMethod()).CreateDelegate(typeof(Func<object, object>));
            SetValue = (Action<object, object>)CreateDynamicMethod(propertyInfo.GetSetMethod()).CreateDelegate(typeof(Action<object, object>));
        }

        public static PropertyInvoker Create<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var lambda = (LambdaExpression)propertyExpression;

            var memberExpression = lambda.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("propertyExpression is not an instance of MemberExpression.", "propertyExpression");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("memberExpression.Member is not an instance of PropertyInfo.", "propertyExpression");
            }

            return new PropertyInvoker(propertyInfo);
        }

        public Func<object, object> GetValue { get; private set; }
        public Action<object, object> SetValue { get; private set; }

        private static DynamicMethod CreateDynamicMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();

            // We want all parameters to be of type object so that we can cast its created delegate to a Func<object, object> or Action<object, object>.
            var types = Enumerable.Repeat(typeof(object), parameters.Length + 1).ToArray();

            var dynamicMethod = new DynamicMethod(
                method.Name + "_Invoker",
                method.ReturnType == typeof(void) ? null : typeof(object),
                types,
                typeof(PropertyInvoker));

            ILGenerator il = dynamicMethod.GetILGenerator();

            for (int i = 0; i < types.Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            if (parameters.Length == 1 && parameters[0].ParameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, parameters[0].ParameterType);
            }

            il.EmitCall(OpCodes.Callvirt, method, null);

            if (method.ReturnType.IsValueType && parameters.Length == 0)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            return dynamicMethod;
        }
    }
}