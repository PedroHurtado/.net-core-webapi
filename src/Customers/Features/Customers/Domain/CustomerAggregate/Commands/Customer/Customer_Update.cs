namespace Customers.Features.Customers.Domain.CustomerAggregate;

public record UpdateCustomerCommand(
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string EstablishmentType,
    string DefaultCulture,
    string TimeZoneId,
    string[] CuisineTypes,
    string[] ServiceAmenities,
    string[] DietaryOptions
);

public partial class Customer
{
    [Injectable(ServiceLifetime.Singleton)]
    public class Update(
        IValidator<Customer> customerValidator
    ) : AbstractModifyCommand<UpdateCustomerCommand, Customer>
    {
        public override Customer Execute(Customer customer, UpdateCustomerCommand command)
        {
            customer.Name = command.Name;
            customer.Slug = command.Slug;
            customer.Description = command.Description;
            customer.LogoUrl = command.LogoUrl;
            customer.EstablishmentType = command.EstablishmentType;
            customer.DefaultCulture = command.DefaultCulture;
            customer.TimeZoneId = command.TimeZoneId;

            customer._cuisineTypes.Clear();
            foreach (var cuisine in command.CuisineTypes)
            {
                customer._cuisineTypes.Add(cuisine);
            }

            customer._serviceAmenities.Clear();
            foreach (var amenity in command.ServiceAmenities)
            {
                customer._serviceAmenities.Add(amenity);
            }

            customer._dietaryOptions.Clear();
            foreach (var option in command.DietaryOptions)
            {
                customer._dietaryOptions.Add(option);
            }

            return customerValidator.ValidateOrThrow(customer);
        }
    }
}
