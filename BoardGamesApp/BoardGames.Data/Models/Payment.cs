using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("payments")]
public class Payment
{
    public Payment(decimal paidAmount, int requestId, int clientId, int ownerId)
    {
        PaidAmount = paidAmount;
        RequestId = requestId;
        ClientId = clientId;
        OwnerId = ownerId;
        PaymentState = 0;
    }

    public Payment() { }

    [Key]
    [Column("id")]
    public int TransactionIdentifier { get; set; }

    [Column("paid_amount")]
    public decimal PaidAmount { get; set; }

    [Column("payment_method")]
    public string? PaymentMethod { get; set; }

    [Column("date_of_transaction")]
    public DateTime? DateOfTransaction { get; set; }

    [Column("date_confirmed_buyer")]
    public DateTime? DateConfirmedBuyer { get; set; }

    [Column("date_confirmed_seller")]
    public DateTime? DateConfirmedSeller { get; set; }

    [Column("payment_state")]
    public int PaymentState { get; set; }

    [Column("receipt_file_path")]
    public string? ReceiptFilePath { get; set; }

    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("owner_id")]
    public int OwnerId { get; set; }

    [ForeignKey("RequestId")]
    public Rental? Request { get; set; }

    [ForeignKey("ClientId")]
    public User? Client { get; set; }

    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }
}

public class HistoryPayment : Payment
{
    public HistoryPayment(decimal paidAmount, int requestId, int clientId, int ownerId, string? gameName, string? ownerName)
        : base(paidAmount, requestId, clientId, ownerId)
    {
        GameName = gameName;
        OwnerName = ownerName;
    }

    public HistoryPayment() : base() { }

    [Column("game_name")]
    public string? GameName { get; set; }

    [Column("owner_name")]
    public string? OwnerName { get; set; }

    [NotMapped]
    public DateTime? RentalStartDate { get; set; }

    [NotMapped]
    public DateTime? RentalEndDate { get; set; }

    [NotMapped]
    public string? ClientName { get; set; }
}