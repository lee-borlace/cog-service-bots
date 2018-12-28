using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDataLayer
{
    public class CosmosConfig
    {
        public string DatabaseId { get; set; }

        public string CollectionId { get; set; }

        public string Endpoint { get; set; }

        public string AuthKey { get; set; }

    }
}
