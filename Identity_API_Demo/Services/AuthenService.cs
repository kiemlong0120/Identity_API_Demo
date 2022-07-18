using Identity_API_Demo.Entity;
using Identity_API_Demo.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Identity_API_Demo.Services
{
    public class AuthenService : IAuthenService
    {
        #region Init

        public IdentityDemoDbContext _db;
        #endregion

        #region Constructor 
        public AuthenService(IdentityDemoDbContext db)
        {
            _db = db;
        }
        #endregion

        #region Service
        /// <summary>
        /// Service get all customer in database (for ADMIN check).
        /// </summary>
        /// <returns>List student from query</returns>
        public List<Customer> GetAllCustomer()
        {
            var query = _db.Users;

            return query.ToList();
        }

        /// <summary>
        /// Service get single customer in database (for ADMIN check).
        /// </summary>
        /// <param name="id">Id = Username from input</param>
        /// <returns></returns>
        public Customer GetCustomer(string Username)
        {
            return _db.Users.SingleOrDefault(u => u.UserName == Username);
        }

        /// <summary>
        /// Service to get role name from Roles table and set to RoleName of Customer
        /// </summary>
        /// <param name="id">Customer id</param>
        public void SetRoleName(string id)
        {
            var Custome = _db.Users.SingleOrDefault(u => u.Id == id);

            // Get roleid from database by CustomerId
            string RoleId = _db.UserRoles.Where(r => r.UserId == id).Select(r => r.RoleId).Single();

            // Get roleName from database by RoleId
            string roleName = _db.Roles.Where(r => r.Id == RoleId).Select(r => r.Name).Single();

            // Ser Role name for customer (IdentityUser)
            Custome.RoleName = roleName;
            _db.SaveChanges();
        }

        /// <summary>
        /// Service get all role in database (for ADMIN check).
        /// </summary>
        /// <param name="id"></param>
        public List<string> ShowAllRole()
        {
            return _db.Roles.Select(r => r.Name).ToList();
        }
        
        /// <summary>
        /// Service delete user with id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteUsers(Customer customer)
        {

            _db.Users.Remove(customer);
            _db.SaveChanges();
        }


        public Customer UpdateUser(string userName, Customer customer)
        {
            var cus = _db.Users.SingleOrDefault(u => u.UserName == userName);
            if (cus != null)
            {
                cus.Email = customer.Email;
                cus.Name = customer.Name;
                cus.Address = customer.Address;
                cus.PhoneNumber = customer.PhoneNumber;


                _db.SaveChanges();
                return cus;
            }
            return null;
        }

        public Customer GetCustomerByUserName(string userName)
        {
            return _db.Users.FirstOrDefault(u => u.UserName == userName);
        }
        #endregion

    }
}
