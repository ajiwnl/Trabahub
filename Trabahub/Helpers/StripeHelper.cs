using Stripe;

namespace Trabahub.Helpers
{
    public class StripeHelper
    {
        public static async Task<decimal> CalculateTotalChargesFromStripe()
        {
            var charges = new ChargeService();

            // Retrieve all charges from Stripe
            var options = new ChargeListOptions { Limit = 9999 }; // Adjust the limit based on your needs
            var chargeList = await charges.ListAsync(options);

            // Sum the amount of all charges
            decimal totalChargesAmount = 0;
            foreach (var charge in chargeList)
            {
                totalChargesAmount += charge.Amount / 100m; // Convert amount from cents to dollars
            }

            return totalChargesAmount;
        }
    }

}
