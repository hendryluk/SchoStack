using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using FubuCore.Reflection;
using HtmlTags;
using SchoStack.Web.Extensions;

namespace SchoStack.Web.Html
{
    public class LoopItem<TModel, T>
    {
        private readonly Func<TModel, int, T> _listFunc;
        private readonly HtmlHelper<TModel> _htmlHelper;
        private readonly Expression<Func<TModel, T>> _currentIndexedExpression;
        
        public T Item { get; set; }
        public LoopInfo Info { get; set; }

        public struct LoopInfo
        {
            public int Index;
            public bool IsLast;
            public bool IsFirst;
            public bool IsEven;
            public bool IsOdd;
        }

        public LoopItem(Expression<Func<TModel, T>> currentIndexedExpression, Func<TModel, int, T> listFunc, HtmlHelper<TModel> htmlHelper, T item, LoopInfo loopInfo)
        {
            Item = item;
            Info = loopInfo;
            _listFunc = listFunc;
            _htmlHelper = htmlHelper;
            _currentIndexedExpression = currentIndexedExpression;
        }

        public HtmlTag Display(Expression<Func<T, object>> propExpression)
        {
            var expression = CombineExpression(propExpression);
            return _htmlHelper.Display(expression);
        }

        public HtmlTag Input(Expression<Func<T, object>> propExpression)
        {
            var expression = CombineExpression(propExpression);
            return _htmlHelper.Input(expression);
        }

        public HtmlTag Label(Expression<Func<T, object>> propExpression)
        {
            var expression = CombineExpression(propExpression);
            return _htmlHelper.Label(expression);
        }

        private static ConcurrentDictionary<Type, object> cache = new ConcurrentDictionary<Type, object>();

        public IEnumerable<LoopItem<TModel, TData>> Loop<TData>(Expression<Func<T, IEnumerable<TData>>> listExpression)
        {
            var newExpression = CombineExpression(listExpression);
            
            var compiledFunc2 = (Tuple<Func<T, IEnumerable<TData>>, Func<TModel, int, TData>>) cache.GetOrAdd(typeof (TData), y =>
            {
                var accessor = GetCurrentIndexedExpressionWithIntParam(newExpression).Compile();
                var tuple = new Tuple<Func<T, IEnumerable<TData>>, Func<TModel, int, TData>>(listExpression.Compile(), accessor);
                return tuple;
            });

            var enumerable = compiledFunc2.Item1(_listFunc.Invoke(_htmlHelper.ViewData.Model, Info.Index));

            return LoopItem<TModel, TData>.LoopItems(_htmlHelper, newExpression, compiledFunc2.Item2, enumerable);
        }

        private Expression<Func<TModel, TProp>> CombineExpression<TProp>(Expression<Func<T, TProp>> propExpression)
        {
            var newExpression = _currentIndexedExpression.Combine(propExpression);
            return newExpression;
        }

        public static Expression<Func<TModel, int, TData>> GetCurrentIndexedExpressionWithIntParam<TData>(Expression<Func<TModel, IEnumerable<TData>>> listExpression)
        {
            var intParameter = Expression.Parameter(typeof(int), "i");
            var memberExpression = listExpression.GetMemberExpression(false);
            var methodInfo = GetMethodInfo(memberExpression);
            var methodCallExpression = MethodCallExpression(memberExpression, intParameter, methodInfo);
            var parameterExpression = listExpression.Parameters.First(x => x.Type == typeof(TModel));
            var expression = Expression.Lambda<Func<TModel, int, TData>>(methodCallExpression, parameterExpression, intParameter);
            return expression;
        }

        private static Func<int, Expression<Func<TModel, TData>>> GetCurrentIndexedExpression<TData>(Expression<Func<TModel, IEnumerable<TData>>> listExpression)
        {
            var memberExpression = listExpression.GetMemberExpression(false);
            var methodInfo = GetMethodInfo(memberExpression);

            return index =>
            {
                var exp = MethodCallExpression(memberExpression, Expression.Constant(index), methodInfo);
                var parameterExpression = listExpression.Parameters.First(x => x.Type == typeof(TModel));
                var expression = Expression.Lambda<Func<TModel, TData>>(exp, parameterExpression);
                return expression;
            };
        }

        private static MethodInfo GetMethodInfo(MemberExpression memberExpression)
        {
            var methodInfo = ((PropertyInfo) memberExpression.Member).PropertyType.GetMethod("get_Item");
            return methodInfo;
        }

        private static MethodCallExpression MethodCallExpression(MemberExpression memberExpression, Expression parameterExpression, MethodInfo methodInfo)
        {
            var methodCallExpression = Expression.Call(memberExpression, methodInfo, new[] {parameterExpression});
            return methodCallExpression;
        }

        public static IEnumerable<LoopItem<TModel, TData>> LoopItems<TData>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IEnumerable<TData>>> listExpression, Func<TModel, int, TData> listFunc, IEnumerable<TData> enumerable)
        {
            var i = 0;
            var enumerator = enumerable.GetEnumerator();
            var next = enumerator.MoveNext();

            var indexedExp = GetCurrentIndexedExpression(listExpression);

            while (next)
            {
                var current = enumerator.Current;
                next = enumerator.MoveNext();

                var loopInfo = new LoopItem<TModel, TData>.LoopInfo()
                {
                    Index = i,
                    IsOdd = i % 2 != 0,
                    IsEven = i % 2 == 0,
                    IsFirst = i == 0,
                    IsLast = next == false
                };

                yield return new LoopItem<TModel, TData>(indexedExp(i), listFunc, htmlHelper, current, loopInfo);

                i++;
            }
        }
    }
}