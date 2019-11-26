using System;
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
        public JobDataEntry Entry { get; set; }
    }

    [DataContract]
    class RepairJobApiFullRequest
    {
        [DataMember]
        public JobDataEntry ContainedEntry;

        [DataMember]
        public int UserId;

        /**
         * <summary>JSON String. Format provided in LoggedTokens</summary>
         */
        [DataMember]
        public string LoginToken;

        [DataMember]
        public string AuthToken;

        [DataMember]
        public int Duplicate;
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
                if (!ctx.Request.HasEntityBody)
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "No Body");
                    return;
                }
                RepairJobApiFullRequest entry = JsonDataObjectUtil<RepairJobApiFullRequest>.ParseObject(ctx);
                if (!ValidateFullRequest(entry))
                {
                    WriteBodyResponse(ctx, 400, "Bad Request", "Incorrect Format");
                    return;
                }
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
                    if (entry.ContainedEntry.JobId.Contains('<'))
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
                    else
                    {
                        //test if there exists similar
                        string whereString = "Make =\"" + entry.ContainedEntry.Make + "\" AND " + "Model =\"" + entry.ContainedEntry.Model+"\"";
                        //whereString += "AND"+entry.ContainedEntry.Year+">="+(entry.ContainedEntry.Year-2)+"AND"+entry.ContainedEntry.Year+"<="+(entry.ContainedEntry.Year+2);
                        List<JobDataEntry> dataCollectionsWhere = connection.GetDataEntriesWhere(mappedUser.Company, whereString, true);
                        List<JobDataEntry> data2 = connection.GetDataEntriesWhere(mappedUser.Company, whereString, false);
                        foreach(JobDataEntry x in data2)
                        {
                            dataCollectionsWhere.Add(x);
                        }
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
                    }
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


        private static float CalcSimilarity(JobDataEntry query, JobDataEntry other)
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

        private List<EntrySimilarity> getSimilar(JobDataEntry Query, List<JobDataEntry> potentials, int maxRet)
        {
            Dictionary<float, List<EntrySimilarity>> distanceMappings = new Dictionary<float, List<EntrySimilarity>>();
            HashSet<float> keys = new HashSet<float>();
            List<EntrySimilarity> ret = new List<EntrySimilarity>();
            foreach (JobDataEntry other in potentials)
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


        private bool ValidateJobDataEntry(JobDataEntry entryIn)
        {
            if (entryIn == null)
                return false;
            if (entryIn.JobId == "")
                return false;
            if (entryIn.Make == "")
                return false;
            if (entryIn.Model == "")
                return false;
            if (entryIn.Complaint == "")
                return false;
            if (entryIn.Problem == "")
                return false;
            return true;
        }

        private bool ValidateFullRequest(RepairJobApiFullRequest req)
        {
            if (!ValidateJobDataEntry(req.ContainedEntry))
                return false;
            if (req.LoginToken == "")
                return false;
            if (req.UserId == -1)
                return false;
            return true;
        }
    }
}
