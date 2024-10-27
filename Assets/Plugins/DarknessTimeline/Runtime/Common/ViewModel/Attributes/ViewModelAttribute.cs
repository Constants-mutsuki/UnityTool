using System;

namespace Darkness
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ViewModelAttribute : Attribute
    {
        public Type modelType;

        public ViewModelAttribute(Type modelType)
        {
            this.modelType = modelType;
        }
    }
}
