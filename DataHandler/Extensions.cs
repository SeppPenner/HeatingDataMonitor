﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataHandler
{
    public static class Extensions
    {
        public static string GetStringFromDateTime(this DateTime value) {
            return value.ToString("dd.MM.yyyy HH:mm:ss");
        }

        #region GetStringFromNullable

        public static string GetString<T>(this T? value) where T : struct
        {
            if (value.HasValue) return value.Value.ToString();
            return "#####";
        }

        public static string GetString<T>(this T? value, string unit) where T : struct
        {
            if (value.HasValue) return $"{value.Value.ToString()} {unit}";
            return "#####";
        }

        public static string GetStringShort<T>(this T? value, string unit) where T : struct
        {
            if (value.HasValue) return $"{value.Value.ToString()} {unit}";
            return "###";
        }

        #endregion

        #region IsGenericTypeOf

        public static bool IsGenericTypeOf(this Type t, Type genericDefinition)
        {
            return IsGenericTypeOf(t, genericDefinition, out _);
        }

        public static bool IsGenericTypeOf(this Type t, Type genericDefinition, out Type[] genericParameters)
        {
            genericParameters = new Type[] { };
            if (!genericDefinition.IsGenericType)
            {
                return false;
            }

            bool isMatch = t.IsGenericType && t.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition();
            if (!isMatch && t.BaseType != null)
            {
                isMatch = IsGenericTypeOf(t.BaseType, genericDefinition, out genericParameters);
            }

            if (!isMatch && genericDefinition.IsInterface && t.GetInterfaces().Any())
            {
                foreach (var i in t.GetInterfaces())
                {
                    if (i.IsGenericTypeOf(genericDefinition, out genericParameters))
                    {
                        isMatch = true;
                        break;
                    }
                }
            }

            if (isMatch && !genericParameters.Any())
            {
                genericParameters = t.GetGenericArguments();
            }

            return isMatch;
        }

        #endregion
    }
}
