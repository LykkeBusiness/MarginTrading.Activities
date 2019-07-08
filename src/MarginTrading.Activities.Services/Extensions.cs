// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;

namespace MarginTrading.Activities.Services
{
    public static class Extensions
    {
        private const int DefaultDigitPrecision = 2;
        
        public static string ToUiString(this decimal? value, int? precision)
        {
            return value.HasValue 
                ? value.Value.ToUiString(precision) 
                : ((decimal) 0).ToUiString(precision);
        }
        
        public static string ToUiString(this decimal value, int? precision)
        {
            precision = precision ?? DefaultDigitPrecision;
            
            var sharps = precision > 0
                ? "." + string.Join(string.Empty, Enumerable.Repeat("#", precision.Value))
                : "";
            
            return value.ToString($"0{sharps}");
        }
    }
}