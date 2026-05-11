namespace TarvelAI.DTOs.Trip;

public sealed class TripBillingInput
{
    public string LegalName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public static bool HasAllRequiredFields(TripBillingInput b) =>
        !string.IsNullOrWhiteSpace(b.LegalName)
        && !string.IsNullOrWhiteSpace(b.AddressLine1)
        && !string.IsNullOrWhiteSpace(b.City)
        && !string.IsNullOrWhiteSpace(b.PostalCode)
        && !string.IsNullOrWhiteSpace(b.Country)
        && !string.IsNullOrWhiteSpace(b.Email);
}
