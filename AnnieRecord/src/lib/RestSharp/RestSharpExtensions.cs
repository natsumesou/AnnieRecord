using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

public static class RestSharpExtensions {
    public static String toSerializableString(this IRestResponse response) {
        return response.Request.Method + " " + response.ResponseUri.AbsolutePath;
    }
}