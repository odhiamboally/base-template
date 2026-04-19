using BT.SharedKernel.Dtos.Lookups;
using BT.SharedKernel.Validation.Validators.Common;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace BT.SharedKernel.Validation.Validators.Lookups;



public class GetLookupRequestValidator : Validator<GetLookupRequest>
{
    public GetLookupRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.LookupType)
            .IsInEnum()
            .WithMessage("Invalid lookup type requested.");
    }

}
