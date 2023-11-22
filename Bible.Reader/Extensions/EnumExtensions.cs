using System;
using System.ComponentModel;

namespace Bible.Reader.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum genericEnum)
        {
            var enumType = genericEnum.GetType();
            var enumName = genericEnum.ToString();
            var memberInfo = enumType.GetMember(enumName);
            if (memberInfo?.Length > 0)
            {
                var customAttributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (customAttributes?.Length > 0 && customAttributes[0] is DescriptionAttribute enumAttribute)
                    enumName = enumAttribute.Description;
            }
            return enumName;
        }
    }
}