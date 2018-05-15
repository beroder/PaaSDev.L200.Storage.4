using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using Microsoft.WindowsAzure.Storage.Auth;
using System.Diagnostics;
using System.IO;

namespace _WriteToTable
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            try
            {
                string sasString = GetAccountSASToken();

                StorageCredentials accountSAS = new StorageCredentials(sasString);
                CloudStorageAccount accountWithSAS = new CloudStorageAccount(accountSAS, "storagelab4m4vncfbha6qwi", endpointSuffix: null, useHttps: true);

                // Create a new customer entity.
                CustomerEntity customer1 = new CustomerEntity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                customer1.Email = "Walter@contoso.com";
                customer1.PhoneNumber = "425-555-0101";

                // Create the table client.
                CloudTableClient tableClient = accountWithSAS.CreateCloudTableClient();

                // Get a reference to a table named "peopleTable" and creat it if it doesn't already exist
                CloudTable peopleTable = tableClient.GetTableReference("peopleTable");
                peopleTable.CreateIfNotExists();

                // Create the TableOperation that inserts the customer entity.
                Console.WriteLine("Writing to table...");
                TableOperation insertOperation = TableOperation.Insert(customer1);
                Console.WriteLine("Writing to success!");
                Console.WriteLine("TimeStamp: {0} UTC", DateTime.Now.ToUniversalTime().ToString());

                // Execute the insert operation.
                peopleTable.Execute(insertOperation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                Console.WriteLine("TimeStamp: {0} UTC", DateTime.Now.ToUniversalTime().ToString());
            }
        }

        static string GetAccountSASToken()
        {
            // To create the account SAS, you need to use your shared key credentials. Modify for your account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a new access policy for the account.
            SharedAccessAccountPolicy policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Create | SharedAccessAccountPermissions.Add | SharedAccessAccountPermissions.Update | SharedAccessAccountPermissions.List,
                Services = SharedAccessAccountServices.Table,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.Subtract(new TimeSpan(4, 0, 0)),
                Protocols = SharedAccessProtocol.HttpsOnly
            };

            return storageAccount.GetSharedAccessSignature(policy);
        }

    }

    public class CustomerEntity : TableEntity
    {
        public CustomerEntity(string lastName, string firstName)
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public CustomerEntity() { }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }
}
