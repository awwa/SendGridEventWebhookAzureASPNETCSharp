using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace SendGridEventWebhook.Controllers
{
	public sealed class DocumentDb
	{
		private static DocumentDb singleInstance = new DocumentDb();

		private static DocumentClient client;

		//Assign a id for your database & collection 
		private static readonly string databaseId = ConfigurationManager.AppSettings["DatabaseId"];
		private static readonly string collectionId = ConfigurationManager.AppSettings["CollectionId"];

		//Read the DocumentDB endpointUrl and authorisationKeys from config
		//These values are available from the Azure Management Portal on the DocumentDB Account Blade under "Keys"
		//NB > Keep these values in a safe & secure location. Together they provide Administrative access to your DocDB account
		private static readonly string endpointUrl = ConfigurationManager.AppSettings["EndPointUrl"];
		private static readonly string authorizationKey = ConfigurationManager.AppSettings["AuthorizationKey"];

		private static Database database;
		private static DocumentCollection collection;
		private static StoredProcedure sproc;

		public static DocumentDb GetInstance()
		{
			return singleInstance;
		}

		private DocumentDb()
		{

		}

		public async Task Init()
		{
			//Get a Document client
			client = new DocumentClient(new Uri(endpointUrl), authorizationKey);

			//Get, or Create, the Database
			database = await GetOrCreateDatabaseAsync(databaseId);

			//Get, or Create, the Document Collection
			collection = await GetOrCreateCollectionAsync(database.SelfLink, collectionId);

			//Reflesh Stored Procedure
			sproc = await TryRefleshStoredProcedure(collection.SelfLink);
		}

		public async Task RunBulkImport(string srcJson)
		{
			List<dynamic> dstJsonList = new List<dynamic>();
			List<dynamic> srcJsonList = JsonConvert.DeserializeObject<List<dynamic>>(srcJson);
			foreach (dynamic json in srcJsonList)
			{
				Guid guidValue = Guid.NewGuid();
				json["id"] = guidValue.ToString();
				dstJsonList.Add(json);
			}
			StoredProcedureResponse<int> scriptResult = await client.ExecuteStoredProcedureAsync<int>(sproc.SelfLink, dstJsonList);
			Console.WriteLine("Finish insert: {0}", scriptResult.Response);
		}

		private static async Task<StoredProcedure> TryRefleshStoredProcedure(string colSelfLink)
		{
			string path = HttpContext.Current.Server.MapPath("~/App_Data/BulkImport.js");
			string body = File.ReadAllText(path);
			StoredProcedure sproc = new StoredProcedure
			{
				Id = "BulkImport",
				Body = body
			};

			await TryDeleteStoredProcedure(colSelfLink, sproc.Id);
			sproc = await client.CreateStoredProcedureAsync(colSelfLink, sproc);
			return sproc;
		}

		/// <summary>
		/// If a Stored Procedure is found on the DocumentCollection for the Id supplied it is deleted
		/// </summary>
		/// <param name="colSelfLink">DocumentCollection to search for the Stored Procedure</param>
		/// <param name="sprocId">Id of the Stored Procedure to delete</param>
		/// <returns></returns>
		private static async Task TryDeleteStoredProcedure(string colSelfLink, string sprocId)
		{
			StoredProcedure sproc = client.CreateStoredProcedureQuery(colSelfLink).Where(s => s.Id == sprocId).AsEnumerable().FirstOrDefault();
			if (sproc != null)
			{
				await client.DeleteStoredProcedureAsync(sproc.SelfLink);
			}
		}

		/// <summary>
		/// Get a DocuemntCollection by id, or create a new one if one with the id provided doesn't exist.
		/// </summary>
		/// <param name="id">The id of the DocumentCollection to search for, or create.</param>
		/// <returns>The matched, or created, DocumentCollection object</returns>
		private static async Task<DocumentCollection> GetOrCreateCollectionAsync(string dbLink, string id)
		{
			DocumentCollection collection = client.CreateDocumentCollectionQuery(dbLink).Where(c => c.Id == id).ToArray().FirstOrDefault();
			if (collection == null)
			{
				collection = new DocumentCollection { Id = id };
				collection.IndexingPolicy.IncludedPaths.Add(new IndexingPath { IndexType = IndexType.Range, NumericPrecision = 5, Path = "/" });

				collection = await client.CreateDocumentCollectionAsync(dbLink, collection);
			}

			return collection;
		}

		/// <summary>
		/// Get a Database by id, or create a new one if one with the id provided doesn't exist.
		/// </summary>
		/// <param name="id">The id of the Database to search for, or create.</param>
		/// <returns>The matched, or created, Database object</returns>
		private static async Task<Database> GetOrCreateDatabaseAsync(string id)
		{
			Database database = client.CreateDatabaseQuery().Where(db => db.Id == id).ToArray().FirstOrDefault();
			if (database == null)
			{
				database = await client.CreateDatabaseAsync(new Database { Id = id });
			}

			return database;
		}

	}
}