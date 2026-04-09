using System.ComponentModel.DataAnnotations;

namespace KMFlow.Domain.Enums;

public enum NotificationType
{
    [Display(Name = "Knowledge Submitted")]
    KnowledgeSubmitted = 0,

    [Display(Name = "Knowledge Approved")]
    KnowledgeApproved = 1,

    [Display(Name = "Knowledge Published")]
    KnowledgePublished = 2,

    [Display(Name = "Knowledge Rejected")]
    KnowledgeRejected = 3,

    [Display(Name = "Role Changed")]
    RoleChanged = 4
}
