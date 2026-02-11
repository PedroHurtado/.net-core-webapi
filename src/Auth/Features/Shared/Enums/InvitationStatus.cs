namespace Auth.Features.Shared.Enums;

/// <summary>
/// Defines the current status of a membership invitation.
/// </summary>
/// <remarks>
/// Tracks the lifecycle of an invitation from creation to resolution.
/// </remarks>
public enum InvitationStatus
{
    /// <summary>
    /// The invitation has been sent and is awaiting a response.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The invitation has been accepted by the invitee.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// The invitation has been cancelled by the owner.
    /// </summary>
    Cancelled = 3
}
