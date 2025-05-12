using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Mimeo.DynamicUI.Extensions
{
    public static class LinqExtensions
    {
        public static Expression<Func<TNew>> Cast<TOld, TNew>(Expression<Func<TOld>> expression)
        {
            return Expression.Lambda<Func<TNew>>(Expression.Convert(expression.Body, typeof(TNew)), expression.Parameters);
        }

        public static PropertyInfo GetSelectedProperty<TProperty>(this Expression<Func<TProperty>> expression)
            => GetSelectedProperty((LambdaExpression)expression);

        public static PropertyInfo GetSelectedProperty<TObject, TProperty>(this Expression<Func<TObject, TProperty>> expression)
            => GetSelectedProperty((LambdaExpression)expression);

        public static MemberInfo GetSelectedMember(this LambdaExpression lambda)
        {
            var baseExpression = DereferenceUnary(lambda.Body);
            if (baseExpression is not MemberExpression memberExpression)
            {
                throw new ArgumentException($"Expression '{baseExpression}' does not select a class member", nameof(lambda));
            }

            return memberExpression.Member;
        }

        public static string? GetSelectedMemberFullName(this LambdaExpression lambda)
        {
            var baseExpression = DereferenceUnary(lambda.Body);
            var memberExpression = baseExpression as MemberExpression
                ?? throw new ArgumentException($"Expression '{baseExpression}' does not select a class member", nameof(lambda));
            
            var memberStack = new Stack<string>();
            while (memberExpression != null)
            {
                memberStack.Push(memberExpression.Member.Name);
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            return memberStack.Count == 0 ? null : string.Join(".", memberStack);
        }

        private static Expression DereferenceUnary(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                return DereferenceUnary(unaryExpression.Operand);
            }

            return expression;
        }

        public static PropertyInfo GetSelectedProperty(this LambdaExpression lambda)
        {
            var memberInfo = lambda.GetSelectedMember();
            if (memberInfo is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException($"Expression '{lambda}' does not select a property", nameof(lambda));
            }

            return propertyInfo;
        }

        public static bool Any(this IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            return enumerator.MoveNext();
        }
    }
}
