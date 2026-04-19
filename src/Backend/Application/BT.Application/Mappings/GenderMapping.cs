using BT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.Application.Mappings;

public static class GenderMapping
{
    public static Gender MapFromString(this string gender)
    {
        return gender switch
        {
            "Male" => Gender.Male,
            "Female" => Gender.Female,
            _ => Gender.Other
        };
    }

    public static string MapToString(this Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Male",
            Gender.Female => "Female",
            _ => "Other"
        };
    }
}
