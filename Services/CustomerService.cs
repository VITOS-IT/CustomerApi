using CustomerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CustomerAPI.Services
{
    public class CustomerService
    {
        private readonly CustomerContext _context;

        public CustomerService(CustomerContext context)
        {
            _context = context;
        }

        public CustomerCreationStatus Add(Customer customer)
        {
            CustomerCreationStatus customerCreationStatus = new CustomerCreationStatus();
            try
            {
                Customer customer1 = _context.customers.FirstOrDefault(p => p.Email == customer.Email);
                if (customer1 != null) 
                {
                    customerCreationStatus.CustomerEmail = customer.Email;
                    customerCreationStatus.Message = "Already exists";
                    _context.creationStatuses.Add(customerCreationStatus);
                    _context.SaveChanges();
                }
                else
                {
                    _context.customers.Add(customer);
                    customerCreationStatus.CustomerEmail = customer.Email;
                    customerCreationStatus.Message = "sucess";
                    _context.creationStatuses.Add(customerCreationStatus);
                    _context.SaveChanges();

                    AddAccount(new AccountDTO()
                    {
                        AccountID = 0,
                        CustomerID = customer.Email,
                        AccountType = "Current",
                        Balance = 0
                    });
                    AddAccount(new AccountDTO()
                    {
                        AccountID = 0,
                        CustomerID = customer.Email,
                        AccountType = "Savings",
                        Balance = 0
                    });
                }
                return customerCreationStatus;
            }
            catch (DbUpdateConcurrencyException Dbce)
            {
                Console.WriteLine(Dbce.Message);
            }
            catch (DbUpdateException Dbe)
            {
                Console.WriteLine(Dbe.Message);
            }
            return null;
        }

        public Customer Get(string email)
        {
            try
            {
            return _context.customers.FirstOrDefault(u => u.Email == email);


            }
            catch (DbUpdateConcurrencyException Dbce)
            {
                Console.WriteLine(Dbce.Message);
            }
            catch (DbUpdateException Dbe)
            {
                Console.WriteLine(Dbe.Message);
            }
            return null;
        }

        public async Task<AccountCreationStatus> AddAccount(AccountDTO accountDTO)
        {
            AccountCreationStatus status = new AccountCreationStatus();
            try
            {
                AccountCreationStatus creationStatus = null;
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri("https://bankcustomerapi.azurewebsites.net/api/account/");
                    client.BaseAddress = new Uri("https://bankaccountapi.azurewebsites.net/api/account/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer");
                    var postTask = await client.PostAsJsonAsync("createAccount", accountDTO);
                    if (postTask.IsSuccessStatusCode)
                    {
                        creationStatus = await postTask.Content.ReadFromJsonAsync<AccountCreationStatus>();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return status;
        }
    }
}
