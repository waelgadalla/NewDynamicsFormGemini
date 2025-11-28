using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DynamicForms.Core.Entities.Data;

/// <summary>
/// Represents a temporary draft of a form module being edited
/// Stored server-side for enterprise security compliance (no client-side storage)
/// </summary>
public class Draft
{
    /// <summary>
    /// Unique identifier for the draft
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// User ID who owns this draft (references AspNetUsers)
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Form module ID being edited
    /// </summary>
    [Required]
    public int ModuleId { get; set; }

    /// <summary>
    /// JSON serialized FormModule data
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string DraftData { get; set; } = string.Empty;

    /// <summary>
    /// When this draft was created (UTC)
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this draft expires and can be deleted (UTC)
    /// Default: 24 hours from creation
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }
}
