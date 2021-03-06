﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Net;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Models.KeywordPrediction;
using OldManInTheShopServer.Models.POSTagger;
using OMISSortingLib;

namespace OldManInTheShopServer.Net.Api
{
    class EntrySimilarity
    {
        public float Difference { get; set; }
        public RepairJobEntry Entry { get; set; }
    }

    [DataContract]
    class RepairJobApiRequest
    {
        [DataMember]
        public RepairJobEntry ContainedEntry = default;

        [DataMember]
        public int UserId = default;

        /**
         * <summary>JSON String. Format provided in LoggedTokens</summary>
         */
        [DataMember]
        public string LoginToken = default;

        [DataMember]
        public string AuthToken = default;

        [DataMember]
        public int Duplicate = default;
    }

    class RepairJobApi : ApiDefinition
    {
#if RELEASE
        public RepairJobApi(int port) : base("https://+:" + port + "/repairjob")
#elif DEBUG
        public RepairJobApi(int port) : base("http://+:" + port + "/repairjob")
#endif
        {
            POST += HandlePostRequest;
        }

        /// <summary>
        /// Request for adding a repair job entry. Documention is found in the Web API Enumeration file
        /// in the /RepairJob tab, starting at row 1
        /// </summary>
        /// <param name="ctx">The HttpListenerContext to respond to</param>
        private void HandlePostRequest(HttpListenerContext ctx)
        {
            try
            {
                #region Input Validation
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                RepairJobApiRequest entry = JsonDataObjectUtil<RepairJobApiRequest>.ParseObject(ctx);
                if (!ValidateFullRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
                #endregion
                //Otherwise we have a valid entry, validate user
                MySqlDataManipulator connection = new MySqlDataManipulator();
                using (connection)
                {
                    bool res = connection.Connect(MySqlDataManipulator.GlobalConfiguration.GetConnectionString());
                    if (!res)
                    {
                        WriteBodyResponse(ctx, 500, "Unexpected ServerError", "Connection to database failed");
                        return;
                    }
                    #region User Validation
                    OverallUser mappedUser = connection.GetUserById(entry.UserId);
                    if(mappedUser==null)
                    {
                        WriteBodyResponse(ctx, 404, "Not Found", "User was not found on the server");
                        return;
                    }
                    if (!UserVerificationUtil.LoginTokenValid(mappedUser, entry.LoginToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Login token was incorrect.");
                        return;
                    }
                    if (!UserVerificationUtil.AuthTokenValid(mappedUser, entry.AuthToken))
                    {
                        WriteBodyResponse(ctx, 401, "Not Authorized", "Auth token was expired or incorrect");
                        return;
                    }
                    #endregion

                    #region Input Sanitation
                    if (entry.ContainedEntry.Complaint.Contains('<'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                        return;
                    }
                    if (entry.ContainedEntry.Problem.Contains('<'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                        return;
                    }
                    if (entry.ContainedEntry.Make.Contains('<'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                        return;
                    }
                    if (entry.ContainedEntry.Model.Contains('<'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                        return;
                    }
                    if (entry.ContainedEntry.JobId.Contains('<'))
                    {
                        WriteBodyResponse(ctx, 400, "Bad Request", "Request contained the < character, which is disallowed due to cross site scripting attacks");
                        return;
                    }
                    #endregion

                    #region Action Handling

                    #region Forced Upload
                    if (!(entry.Duplicate == 0))
                    {
                        //Now that we know the user is good, actually do the addition.
                        res = connection.AddDataEntry(mappedUser.Company, entry.ContainedEntry);
                        if (!res)
                        {
                            WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                            return;
                        }
                        WriteBodylessResponse(ctx, 200, "OK");
                    }
                    #endregion
                    
                    else
                    {
                        //test if there exists similar
                        string whereString = "Make =\"" + entry.ContainedEntry.Make + "\" AND " + "Model =\"" + entry.ContainedEntry.Model+"\"";
                        //whereString += "AND"+entry.ContainedEntry.Year+">="+(entry.ContainedEntry.Year-2)+"AND"+entry.ContainedEntry.Year+"<="+(entry.ContainedEntry.Year+2);
                        List<RepairJobEntry> dataCollectionsWhere = connection.GetDataEntriesWhere(mappedUser.Company, whereString, true);
                        List<RepairJobEntry> data2 = connection.GetDataEntriesWhere(mappedUser.Company, whereString, false);
                        foreach(RepairJobEntry x in data2)
                        {
                            dataCollectionsWhere.Add(x);
                        }
                        #region No Similar Jobs
                        //if none force through
                        if (dataCollectionsWhere.Count==0)
                        {
                            res = connection.AddDataEntry(mappedUser.Company, entry.ContainedEntry);
                            if (!res)
                            {
                                WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                                return;
                            }
                            WriteBodylessResponse(ctx, 200, "OK");
                        }
                        #endregion

                        #region Similar Jobs Return
                        //if yes 409 with similar jobs
                        else
                        {
                            JsonListStringConstructor retConstructor = new JsonListStringConstructor();
                            List<EntrySimilarity> ret = getSimilar(entry.ContainedEntry, dataCollectionsWhere, 3);
                            if (ret.Count == 0)
                            {
                                res = connection.AddDataEntry(mappedUser.Company, entry.ContainedEntry);
                                if (!res)
                                {
                                    WriteBodyResponse(ctx, 500, "Unexpected Server Error", connection.LastException.Message);
                                    return;
                                }
                                WriteBodylessResponse(ctx, 200, "OK");
                            }
                            ret.ForEach(obj => retConstructor.AddElement(ConvertEntrySimilarity(obj)));
                            WriteBodyResponse(ctx, 409, "Conflict" ,retConstructor.ToString(), "application/json");
                            
                            JsonDictionaryStringConstructor ConvertEntrySimilarity(EntrySimilarity e)
                            {
                                JsonDictionaryStringConstructor r = new JsonDictionaryStringConstructor();
                                r.SetMapping("Make", e.Entry.Make);
                                r.SetMapping("Model", e.Entry.Model);
                                r.SetMapping("Complaint", e.Entry.Complaint);
                                r.SetMapping("Problem", e.Entry.Problem);
                                if (e.Entry.Year == -1)
                                    r.SetMapping("Year", "Unknown");
                                else
                                    r.SetMapping("Year", e.Entry.Year);
                                r.SetMapping("Id", e.Entry.Id);
                                return r;
                            }
                        }
                        #endregion

                    }
                    #endregion
                }
            }
            catch (HttpListenerException)
            {
                //HttpListeners dispose themselves when an exception occurs, so we can do no more.
            }
            catch (Exception e)
            {
                WriteBodyResponse(ctx, 500, "Internal Server Error", e.Message);
            }
        }


        private static float CalcSimilarity(RepairJobEntry query, RepairJobEntry other)
        {
            IKeywordPredictor keyPred = NaiveBayesKeywordPredictor.GetGlobalModel();
            AveragedPerceptronTagger tagger = AveragedPerceptronTagger.GetTagger();
            List<String> tokened = SentenceTokenizer.TokenizeSentence(query.Complaint);
            List<List<String>> tagged = tagger.Tag(tokened);
            List<String> InputComplaintKeywords = keyPred.PredictKeywords(tagged);
            tokened = SentenceTokenizer.TokenizeSentence(query.Problem);
            tagged = tagger.Tag(tokened);
            List<String> InputProblemKeywords = keyPred.PredictKeywords(tagged);
            float score = 0;
            tokened = SentenceTokenizer.TokenizeSentence(other.Complaint);
            tagged = tagger.Tag(tokened);
            List<String> JobComplaintKeywords = keyPred.PredictKeywords(tagged);
            tokened = SentenceTokenizer.TokenizeSentence(other.Problem);
            tagged = tagger.Tag(tokened);
            List<String> JobProblemKeywords = keyPred.PredictKeywords(tagged);
            foreach (String keyword in JobComplaintKeywords)
            {
                if (InputComplaintKeywords.Contains(keyword))
                {
                    score++;
                }
            }
            foreach (String keyword in JobProblemKeywords)
            {
                if (InputProblemKeywords.Contains(keyword))
                {
                    score++;
                }
            }
            return (score / (JobComplaintKeywords.Count + JobProblemKeywords.Count));
        }

        private List<EntrySimilarity> getSimilar(RepairJobEntry Query, List<RepairJobEntry> potentials, int maxRet)
        {
            Dictionary<float, List<EntrySimilarity>> distanceMappings = new Dictionary<float, List<EntrySimilarity>>();
            HashSet<float> keys = new HashSet<float>();
            List<EntrySimilarity> ret = new List<EntrySimilarity>();
            foreach (RepairJobEntry other in potentials)
            {
                float dist = CalcSimilarity(Query, other);
                if (!distanceMappings.ContainsKey(dist))
                {
                    keys.Add(dist);
                    distanceMappings.Add(dist, new List<EntrySimilarity>());
                }
                distanceMappings[dist].Add(new EntrySimilarity() { Entry = other, Difference = dist });
            }
            float[] sortedKeys = new float[keys.Count];
            keys.CopyTo(sortedKeys);
            sortedKeys.RadixSort();
            int keyIndex = 0;
            while (ret.Count <= maxRet && keyIndex < sortedKeys.Length)
            {
                ret.AddRange(distanceMappings[sortedKeys[keyIndex]]);
                keyIndex++;
            }
            if (ret.Count < maxRet)
                return ret;
            return ret.GetRange(0, maxRet);
        }


        private bool ValidateRepairJobEntry(RepairJobEntry entryIn)
        {
            if (entryIn == null)
                return false;
            if (entryIn.JobId == null || entryIn.JobId == "")
                return false;
            if (entryIn.Make == null || entryIn.Make == "")
                return false;
            if (entryIn.Model == null || entryIn.Model == "")
                return false;
            if (entryIn.Complaint == null || entryIn.Complaint == "")
                return false;
            if (entryIn.Problem == null || entryIn.Problem == "")
                return false;
            return true;
        }

        private bool ValidateFullRequest(RepairJobApiRequest req)
        {
            if (!ValidateRepairJobEntry(req.ContainedEntry))
                return false;
            if (req.LoginToken == "")
                return false;
            if (req.UserId == -1)
                return false;
            if (req.AuthToken == "")
                return false;
            return true;
        }
    }
}
