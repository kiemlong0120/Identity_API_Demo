using Identity_API_Demo.Entity;
using Microsoft.AspNetCore.Identity;

namespace Identity_API_Demo.Services
{
    public interface IAuthenService
    {
        // Service for admin or testing only
        public List<Customer> GetAllCustomer();
        public Customer GetCustomer(string id);
        public Customer GetCustomerByUserName(string userName);
        public void SetRoleName(string id);
        public List<string> ShowAllRole();
        public void DeleteUsers(Customer customer);

    }
}
