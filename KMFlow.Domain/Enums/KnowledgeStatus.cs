using System.ComponentModel.DataAnnotations;

namespace KMFlow.Domain.Enums;

public enum KnowledgeStatus
{
    [Display(Name = "Draft")]
    Draft = 0,

    [Display(Name = "Pending Review")]
    Pending = 1,

    [Display(Name = "In Review")]
    InReview = 2,

    [Display(Name = "Approved")]
    Approved = 3,

    [Display(Name = "Published")]
    Published = 4,

    [Display(Name = "Rejected")]
    Rejected = 5

}