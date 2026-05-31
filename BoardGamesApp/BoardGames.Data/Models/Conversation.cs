using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("conversations")]
public class Conversation
{
    public Conversation(List<ConversationParticipant> participants)
    {
        Participants = participants;
    }
    public Conversation()
    {
    }

    [Key]
    [Column("id")]
    public int ConversationId { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
