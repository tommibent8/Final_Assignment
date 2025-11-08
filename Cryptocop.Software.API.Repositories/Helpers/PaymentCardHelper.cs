namespace Cryptocop.Software.API.Repositories.Helpers;

public class PaymentCardHelper
{
    public static string MaskPaymentCard(string paymentCardNumber)
    {
        if (string.IsNullOrWhiteSpace(paymentCardNumber) || paymentCardNumber.Length < 4)
        {
            return paymentCardNumber;
        }

        // Mask all but last 4 digits, e.g. 1234567890123456 -> ************3456
        var last4Digits = paymentCardNumber.Substring(paymentCardNumber.Length - 4);
        return new string('*', paymentCardNumber.Length - 4) + last4Digits;
    }
}