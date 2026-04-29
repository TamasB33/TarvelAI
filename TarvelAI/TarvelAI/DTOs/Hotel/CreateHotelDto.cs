using System.ComponentModel.DataAnnotations;

namespace TarvelAI.DTOs.Hotel;

public class CreateHotelDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    public string City { get; set; } = "";

    [Required(ErrorMessage = "Country is required.")]
    [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
    public string Country { get; set; } = "";

    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public double Rating { get; set; }

    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
    public string? ImageUrl { get; set; }
}
