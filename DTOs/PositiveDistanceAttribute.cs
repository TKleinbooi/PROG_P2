using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class PositiveDistanceAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is decimal distance)
            {
                return distance >= 0.01m && distance <= 100000m;
            }

            return false;
        }
    }
}