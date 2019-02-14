﻿using System;


namespace Mirror.Extensions
{
    public static class TryParseExtensions
    {
        public delegate bool ParseDelegate<T>(string value, out T result) where T : struct;

        public static TEnum To<TEnum>(this string value)
            where TEnum : struct, IConvertible, IComparable, IFormattable =>
                TryParse<TEnum>(value, Enum.TryParse);
        
        public static T TryParse<T>(this string value, ParseDelegate<T> parse) 
            where T : struct
            => parse(value as string, out var result) ? result : default(T);
    }
}