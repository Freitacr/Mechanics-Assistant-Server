using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Data.MySql
{
    class TableNameStorage
    {
        public static readonly string OverallUserTable = "overall_user_table";
        public static readonly string CompanyValidatedRepairJobTable = "company_(n)_validated_data";
        public static readonly string CompanyNonValidatedRepairJobTable = "company_(n)_nonvalidated_data";
        public static readonly string CompanyForumTable = "company_(n)_query_(m)_forum_posts";
        public static readonly string CompanyPartsCatalogueTable = "company_(n)_parts_catalogue";
        public static readonly string CompanyPartsRequestTable = "company_(n)_parts_requests";
        public static readonly string CompanySafetyRequestsTable = "company_(n)_safety_requests";
        public static readonly string CompanyPartsListsRequestsTable = "company_(n)_parts_list_requests";
        public static readonly string CompanyIdTable = "company_id_table";
        public static readonly string CompanyProblemKeywordGroupsTable = "company_(n)_problem_keyword_groups";
        public static readonly string CompanyComplaintKeywordGroupsTable = "company_(n)_complaint_keyword_groups";
        public static readonly string CompanyJoinRequestsTable = "company_(n)_join_requests";
        public static readonly string CompanySettingsTable = "company_(n)_settings";
    }
}
