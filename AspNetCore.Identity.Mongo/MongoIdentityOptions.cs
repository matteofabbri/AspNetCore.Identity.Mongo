using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;

namespace AspNetCore.Identity.Mongo
{
	public class MongoIdentityOptions
	{
		public string ConnectionString { get; set; } = "mongodb://localhost/default";
        
	    public string UsersCollection { get; set; } = "Users";
		
	    public string RolesCollection { get; set; } = "Roles";

        public string MigrationCollection { get; set; } = "_Migrations";

		public SslSettings SslSettings { get; set; }

		public Action<ClusterBuilder> ClusterConfigurator { get; set; }
	}
}