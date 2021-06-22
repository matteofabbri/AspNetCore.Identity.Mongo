using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AspNetCore.Identity.Mongo.Mongo
{
    internal static class TypeConverterResolver
    {
        internal static void RegisterTypeConverter<T, TC>() where TC : TypeConverter
        {
            Attribute[] attr = new Attribute[1];
            TypeConverterAttribute vConv = new TypeConverterAttribute(typeof(TC));
            attr[0] = vConv;
            TypeDescriptor.AddAttributes(typeof(T), attr);
        }
    }
}
