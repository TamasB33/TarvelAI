namespace TarvelAI.DTOs.Hotel;

public class HotelDto
{
    public int     Id       { get; set; }
    public string  Name     { get; set; } = "";
    public string  Address  { get; set; } = "";
    public string  City     { get; set; } = "";
    public string  Country  { get; set; } = "";
    public double  Rating   { get; set; }
    public string? ImageUrl { get; set; }
}
