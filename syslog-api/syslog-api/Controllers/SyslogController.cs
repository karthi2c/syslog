using Azure.Identity;
using Kusto.Cloud.Platform.Data;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;



namespace syslog_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyslogController : ControllerBase
    {

        [HttpGet]
        [Route("CheckConnection")]
        public IActionResult CheckConnection()
        {
            try

            {
                //Cluster Uri
                var serviceUri = "https://usazuadxsyslog.eastus.kusto.windows.net";

                //Database Name
                var database = "usazudbsyslog";

                //User Assigned Client Id 
                string? userAssignedClientId = Environment.GetEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID");

                //DefaultAzureCredential -- Uses Managed client Id when deployed in Azure app service and uses microsoft account loggged in visual studio 
                var defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

                //Builds the connection string for ADX.
                var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(serviceUri).WithAadAzureTokenCredentialsAuthentication(defaultAzureCredential);

                using (var queryProvider = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder))
                {
                    // 3. Send a query using the V2 API
                    var query = "test_table | take 5";

                    var properties = new ClientRequestProperties()
                    {
                        ClientRequestId = "Test V2;" + Guid.NewGuid().ToString()
                    };

                    var queryTask = queryProvider.ExecuteQuery(database, query, properties);



                    DataSet dataset = queryTask.ToDataSet();

                    dynamic output = new
                    {
                        Table_0 = dataset.Tables[0],


                    };


                    return Ok(JsonConvert.SerializeObject(output));

                }
            }
            catch (Exception exception)
            {

                var result = StatusCode(StatusCodes.Status500InternalServerError, exception);
                return result;
            }

        }


        [HttpPost]
        [Route("TestApi")]
        public async Task<IActionResult> TestApi(JObject json)
        {       
            JToken? name = json["name"];

            int time = int.Parse(name.ToString());
            
            await Task.Delay(time);

            return Ok(time);

        }
        

    
    }
}
