namespace BT.SharedKernel.Features.Shared.Phone;

public sealed record CountryCallingCode(
    string Iso2,
    string Name,
    string DialCode,
    int NationalNumberMinLength,
    int NationalNumberMaxLength)
{
    public string Flag => CountryCallingCodeCatalog.GetFlagEmoji(Iso2);

    public string DisplayName => $"{Flag} {Name} ({DialCode})";
}
