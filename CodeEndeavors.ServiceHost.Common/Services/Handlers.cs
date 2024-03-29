﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using CodeEndeavors.Extensions;
using System.Net.Http.Headers;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class Handlers
    {
        public static string AquireUserId()
        {
            var userId = "";
            if (Thread.CurrentPrincipal != null && !string.IsNullOrEmpty(Thread.CurrentPrincipal.Identity.Name))
                userId = Thread.CurrentPrincipal.Identity.Name;
            return userId;
        }

        public static void ProcessBasicAuthentication(HttpRequestMessage request, string user, string password, ref string token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes(user + ":" + password))); 
        }

        public static void ProcessOAuth(HttpRequestMessage request, string user, string password, ref string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                var url = request.RequestUri.Scheme + "://" + request.RequestUri.Host + "/token";
                var client = new OAuth2Client(new Uri(url));
                token = client.RequestResourceOwnerPasswordAsync(user, password).Result.AccessToken;
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

    }
}
