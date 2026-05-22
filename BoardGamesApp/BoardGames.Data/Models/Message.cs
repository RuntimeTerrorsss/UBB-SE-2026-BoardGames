using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

[JsonDerivedType(typeof(TextMessage), typeDiscriminator: "text")]
[JsonDerivedType(typeof(ImageMessage), typeDiscriminator: "image")]
[JsonDerivedType(typeof(SystemMessage), typeDiscriminator: "system")]
[JsonDerivedType(typeof(RentalRequestMessage), typeDiscriminator: "rental")]
[JsonDerivedType(typeof(CashAgreementMessage), typeDiscriminator: "cash")]
[Table("messages")]
public abstract class Message
{
    [SetsRequiredMembers]
    public Message(int conversationId, int messageSenderId, int messageReceiverId)
    {
        ConversationId = conversationId;
        MessageSenderId = messageSenderId;
        MessageReceiverId = messageReceiverId;
        MessageSentTime = DateTime.UtcNow;
    }

    public Message() { }

    [Key]
    [Column("id")]
    public int MessageId { get; set; }

    [Column("message_sent_time")]
    public DateTime MessageSentTime { get; set; }

    [Column("message_content_as_string")]
    public string? MessageContentAsString { get; set; }

    [Column("conversation_id")]
    public int ConversationId { get; set; }

    [Column("message_sender_id")]
    public int MessageSenderId { get; set; }

    [Column("message_receiver_id")]
    public int MessageReceiverId { get; set; }

    [ForeignKey("ConversationId")]
    public required Conversation Conversation { get; set; } = null!;

    [ForeignKey("MessageSenderId")]
    public required User Sender { get; set; } = null!;

    [ForeignKey("MessageReceiverId")]
    public required User Receiver { get; set; } = null!;
}

public class TextMessage : Message
{
    [SetsRequiredMembers]
    public TextMessage(int conversationId, int messageSenderId, int messageReceiverId, string textMessageContent)
        : base(conversationId, messageSenderId, messageReceiverId)
    {
        TextMessageContent = textMessageContent;
        MessageContentAsString = textMessageContent;
    }

    public TextMessage() : base() { }

    [Column("text_message_content")]
    public string? TextMessageContent { get; set; }
}

public class ImageMessage : Message
{
    [SetsRequiredMembers]
    public ImageMessage(int conversationId, int messageSenderId, int messageReceiverId, string messageImageUrl)
        : base(conversationId, messageSenderId, messageReceiverId)
    {
        MessageImageUrl = messageImageUrl;
    }

    public ImageMessage() : base() { }

    [Column("message_image_url")]
    public string? MessageImageUrl { get; set; }
}

public class SystemMessage : Message
{
    [SetsRequiredMembers]
    public SystemMessage(int conversationId, int messageSenderId, int messageReceiverId, string messageContent)
        : base(conversationId, messageSenderId, messageReceiverId)
    {
        MessageContent = messageContent;
        MessageContentAsString = messageContent;
    }

    public SystemMessage() : base() { }

    [Column("message_content")]
    public string? MessageContent { get; set; }
}

public class RentalRequestMessage : Message
{
    [SetsRequiredMembers]
    public RentalRequestMessage(int conversationId, int messageSenderId, int messageReceiverId, int rentalRequestId, string? requestContent = null)
        : base(conversationId, messageSenderId, messageReceiverId)
    {
        RentalRequestId = rentalRequestId;
        RequestContent = requestContent;
        IsRequestResolved = false;
        IsRequestAccepted = false;
    }

    public RentalRequestMessage() : base() { }

    [Column("rental_request_id")]
    public int RentalRequestId { get; set; }

    [Column("is_request_resolved")]
    public bool IsRequestResolved { get; set; }

    [Column("is_request_accepted")]
    public bool IsRequestAccepted { get; set; }

    [Column("request_content")]
    public string? RequestContent { get; set; }

    [ForeignKey("RentalRequestId")]
    public Rental? RentalRequest { get; set; }
}

public class CashAgreementMessage : Message
{
    [SetsRequiredMembers]
    public CashAgreementMessage(int conversationId, int messageSenderId, int messageReceiverId, int cashPaymentId)
        : base(conversationId, messageSenderId, messageReceiverId)
    {
        CashPaymentId = cashPaymentId;
        IsCashAgreementResolved = false;
        IsCashAgreementAcceptedByBuyer = false;
        IsCashAgreementAcceptedBySeller = false;
    }

    public CashAgreementMessage() : base() { }

    [Column("cash_payment_id")]
    public int CashPaymentId { get; set; }

    [Column("is_cash_agreement_resolved")]
    public bool IsCashAgreementResolved { get; set; }

    [Column("is_cash_agreement_accepted_by_buyer")]
    public bool IsCashAgreementAcceptedByBuyer { get; set; }

    [Column("is_cash_agreement_accepted_by_seller")]
    public bool IsCashAgreementAcceptedBySeller { get; set; }

    [ForeignKey("CashPaymentId")]
    public Payment? CashPayment { get; set; }
}