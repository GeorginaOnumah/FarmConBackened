using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmConnect.Models
{
    // ─── Enums ───────────────────────────────────────────────────────────────

    public enum UserRole { Farmer, Buyer, Transporter, Admin }
    public enum AccountStatus { Pending, Active, Suspended }
    public enum OrderStatus { Pending, Accepted, Declined, Processing, Dispatched, Delivered, Cancelled }
    public enum PaymentStatus { Pending, Held, Released, Refunded, Failed }
    public enum DeliveryStatus { Unassigned, Assigned, PickedUp, InTransit, Delivered, Failed }
    public enum NotificationType { OrderUpdate, DeliveryUpdate, PaymentUpdate, SystemAlert, Message }

    // ─── User ─────────────────────────────────────────────────────────────────

    public class User
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;
        [Phone, MaxLength(20)] public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Pending;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? LGA { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public FarmerProfile? FarmerProfile { get; set; }
        public BuyerProfile? BuyerProfile { get; set; }
        public TransporterProfile? TransporterProfile { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }

    public class FarmerProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        [MaxLength(200)] public string? FarmName { get; set; }
        public string? FarmDescription { get; set; }
        public string? FarmAddress { get; set; }
        public double? FarmLatitude { get; set; }
        public double? FarmLongitude { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? NINNumber { get; set; }
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class BuyerProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? BusinessType { get; set; }
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class TransporterProfile
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string? VehicleType { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? VehicleCapacity { get; set; }
        public bool IsVerified { get; set; } = false;
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public bool IsAvailable { get; set; } = true;
        public double Rating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public decimal TotalEarnings { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }

    public class UserSession
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
    }

    // ─── Product / Listing ────────────────────────────────────────────────────

    public class ProductCategory
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FarmerProfileId { get; set; }
        public int CategoryId { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal PricePerUnit { get; set; }
        [MaxLength(50)] public string Unit { get; set; } = "kg";
        [Column(TypeName = "decimal(18,2)")] public decimal QuantityAvailable { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public FarmerProfile FarmerProfile { get; set; } = null!;
        public ProductCategory Category { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    public class ProductImage
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsPrimary { get; set; } = false;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Product Product { get; set; } = null!;
    }

    // ─── Market Price ─────────────────────────────────────────────────────────

    public class MarketPrice
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string CropName { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")] public decimal PricePerKg { get; set; }
        [MaxLength(100)] public string Market { get; set; } = string.Empty;
        public string? State { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public Guid UpdatedByAdminId { get; set; }
    }

    // ─── Order ────────────────────────────────────────────────────────────────

    public class Order
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BuyerProfileId { get; set; }
        public Guid FarmerProfileId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal DeliveryFee { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public double? DeliveryLatitude { get; set; }
        public double? DeliveryLongitude { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public BuyerProfile BuyerProfile { get; set; } = null!;
        public FarmerProfile FarmerProfile { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
        public Delivery? Delivery { get; set; }
    }

    public class OrderItem
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal TotalPrice { get; set; }
        public string Unit { get; set; } = "kg";
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

    // ─── Payment (Escrow) ─────────────────────────────────────────────────────

    public class Payment
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaymentGateway { get; set; }
        public string? GatewayReference { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? EscrowReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Order Order { get; set; } = null!;
    }

    // ─── Delivery ─────────────────────────────────────────────────────────────

    public class Delivery
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid? TransporterProfileId { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Unassigned;
        public string? PickupAddress { get; set; }
        public string? DropoffAddress { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string? TrackingCode { get; set; }
        public string? Notes { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? TransporterEarning { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Order Order { get; set; } = null!;
        public TransporterProfile? TransporterProfile { get; set; }
    }

    // ─── Messaging ────────────────────────────────────────────────────────────

    public class Message
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid? OrderId { get; set; }
        [Required] public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }

    // ─── Notifications ────────────────────────────────────────────────────────

    public class Notification
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required] public string Body { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public User User { get; set; } = null!;
    }

    // ─── Reviews ──────────────────────────────────────────────────────────────

    public class Review
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReviewerId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ReviewedUserId { get; set; }
        public Guid OrderId { get; set; }
        [Range(1, 5)] public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Reviewer { get; set; } = null!;
        public Product? Product { get; set; }
    }

    // ─── Audit Log ────────────────────────────────────────────────────────────

    public class AuditLog
    {
        [Key] public long Id { get; set; }
        public Guid? UserId { get; set; }
        [Required, MaxLength(100)] public string Action { get; set; } = string.Empty;
        [MaxLength(100)] public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
