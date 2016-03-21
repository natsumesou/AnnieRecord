using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace AnnieRecord
{
    class API
    {
        private static readonly Lazy<API> lazy = new Lazy<API>(() => new API());

        public RestClient client {
            get;
            private set;
        }
        private String apiKey
        {
            get { return System.Configuration.ConfigurationSettings.AppSettings["API_KEY"];  }
        }

        private API()
        {
            client = new RestClient("https://na.api.pvp.net/");
        }

        public static API Instance
        {
            get { return lazy.Value;  }
        }

        public RestRequest buildRequest(String path, Method method = Method.GET)
        {
            var request = new RestRequest(path, method);
            request.AddParameter("api_key", apiKey);
            return request;
        }
    }
}
