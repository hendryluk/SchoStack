using System;
using System.Collections.Generic;

namespace SchoStack.Web.HtmlTags
{
    public class TagConventions : ITagConventions, IConventionAccessor
    {
        public bool IsAll { get; private set; }
        public IList<Modifier> Modifiers { get; private set; }
        public IList<Builder> Builders { get; private set; }

        public TagConventions() : this(false) { }
        public TagConventions(bool isAll)
        {
            IsAll = isAll;
            Modifiers = new List<Modifier>();
            Builders = new List<Builder>();
        }

        public IConventionAction Always
        {
            get { return new ConventionAction(x => true, Builders, Modifiers); }
        }

        public IConventionAction If(Func<RequestData, bool> condition)
        {
            return new ConventionAction(condition, Builders, Modifiers);
        }

        public IConventionAction If<T>()
        {
            return new ConventionAction(IsAssignable<T>, Builders, Modifiers);
        }

        public IConventionActionAttribute<TAttribute> IfAttribute<TAttribute>() where TAttribute : Attribute
        {
            return new ConventionActionAttribute<TAttribute>(req => req.Accessor.InnerProperty.GetCustomAttributes(typeof(TAttribute), true).Length > 0, Builders, Modifiers);
        }

        public static bool IsAssignable<TProperty>(RequestData x)
        {
            var type = typeof(TProperty);
            var assignable = type.IsAssignableFrom(x.Accessor.PropertyType);
            if (!assignable && type.IsValueType)
            {
                assignable = typeof(Nullable<>).MakeGenericType(type).IsAssignableFrom(x.Accessor.PropertyType);
            }
            return assignable;
        }
    }
}