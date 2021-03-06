﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RawSync
{
    class ProcessedExtensionValidator : ValidationRule
    {
        public static bool HasError { get; set; }

        private string GetValidationError(string s)
        {
            if (s == null)
                return "Value is null";

            if (s.Length == 0)
                return "Value cannot be empty";

            if (s[0] != '.')
                return "1st char must be \'.\'";

            if (s.Length < 4 || s.Length > 5)
                return "Value must be 4 or 5 chars long";

            if (!Char.IsLetter(s[1]) || !Char.IsLetter(s[2]) || !Char.IsLetter(s[3]))
                return "After\".\" must be all letters";

            if (s.Length == 5 && !Char.IsLetter(s[4]))
                return "After\".\" must be all letters";

            return null;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            
            string error = GetValidationError(value.ToString());

            if (error == null)
            {
                HasError = false;
                return ValidationResult.ValidResult;
            }
            else
            {
                HasError = true;
                return new ValidationResult(false, error);
            }
        }
    }
}
