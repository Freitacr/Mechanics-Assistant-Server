﻿using System;
using System.Text;
using System.Net;
using System.IO;

namespace MechanicsAssistantServer.Net.Api
{
    class CertValidationApi : ApiDefinition
    {
        public CertValidationApi() : base("http://+/.well-known/acme-challenge")
        {
            GET += HandleGetRequest;
            OPTIONS += HandleOptionRequest;
            
            //AddAction("put", HandlePutRequest);
            //AddAction("post", HandlePostRequest);
        }

        public void HandleOptionRequest(HttpListenerContext ctxIn)
        {
            ctxIn.Response.StatusCode = 200;
            ctxIn.Response.AddHeader("Access-Control-Allow-Methods", "GET");
            ctxIn.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ctxIn.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctxIn.Response.Close();
        }



        public void HandleGetRequest(HttpListenerContext ctx)
        {
            string fileName = ctx.Request.RawUrl;

            int challengeIndex = fileName.IndexOf("acme-challenge/");
            int fileEnd = fileName.IndexOf('/', challengeIndex + 15);
            if(fileEnd != -1)
            {
                ctx.Response.StatusCode = 404;
                ctx.Response.StatusDescription = "Not Found";
                ctx.Response.OutputStream.Close();
                return;
            }
            fileName = fileName.Substring(challengeIndex+15);
            StreamReader reader = new StreamReader(fileName);
            string authzToken = reader.ReadToEnd();
            reader.Close();
            byte[] token = Encoding.UTF8.GetBytes(authzToken);
            ctx.Response.ContentType = "application/octet-stream";
            ctx.Response.ContentLength64 = token.Length;
            ctx.Response.OutputStream.Write(token, 0, token.Length);
            ctx.Response.Close();
        }
    }
}