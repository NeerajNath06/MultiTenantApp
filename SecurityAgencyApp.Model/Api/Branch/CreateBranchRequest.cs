using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class CreateBranchRequest
{
    [Required]
    [StringLength(50)]
    [Display(Name = "Branch Code")]
    public string BranchCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "Branch Name")]
    public string BranchName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Display(Name = "Pin Code")]
    public string PinCode { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [Phone]
    [StringLength(20)]
    [Display(Name = "Contact Phone")]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    [StringLength(200)]
    [Display(Name = "Contact Email")]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    [Display(Name = "GST Number")]
    public string? GstNumber { get; set; }

    [StringLength(50)]
    [Display(Name = "Labour License Number")]
    public string? LabourLicenseNumber { get; set; }

    [StringLength(20)]
    [Display(Name = "Number Prefix")]
    public string? NumberPrefix { get; set; }

    [Display(Name = "Head Office")]
    public bool IsHeadOffice { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
