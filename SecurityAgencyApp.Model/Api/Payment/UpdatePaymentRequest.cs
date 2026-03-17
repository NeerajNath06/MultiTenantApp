namespace SecurityAgencyApp.Model.Api;

public class UpdatePaymentRequest : CreatePaymentRequest
{
    public Guid Id { get; set; }
}
