using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("conversation_participants")]
public class ConversationParticipant
{
    public ConversationParticipant(int conversationId, int userId)
    {
        ConversationId = conversationId;
        UserId = userId;
        UnreadMessagesCount = 0;
        LastMessageReadTime = null;
    }

    public ConversationParticipant()
    {
    }

    [Column("conversation_id")]
    public int ConversationId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("last_message_read_time")]
    public DateTime? LastMessageReadTime { get; set; }

    [Column("unread_messages_count")]
    public int UnreadMessagesCount { get; set; }

    [ForeignKey("ConversationId")]
    public Conversation? Conversation { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; } 
}