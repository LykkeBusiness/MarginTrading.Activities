// Copyright (c) 2019 Lykke Corp.

using System;

namespace MarginTrading.Activities.Core.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum ToType<TEnum>(this Enum dto)
            where TEnum : struct, IConvertible
        {
            if (!Enum.TryParse(dto.ToString(), out TEnum result))
            {
                throw new NotSupportedException($"Value {dto} is not supported by mapper");
            }

            return result;
        }
    }
}