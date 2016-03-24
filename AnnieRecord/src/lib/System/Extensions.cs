using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
    public static String toSerializableString(this HttpListenerRequest req)
    {
        return req.HttpMethod + " " + req.Url.AbsolutePath;
    }
}