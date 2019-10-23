using System;
using System.Collections.Generic;
using System.Text;

namespace OldManinTheShopServer.Data.MySql
{
    public static class TableCreationDataDeclarationStrings
    {
        public static int RequestHistoryBytesSize = 2048;
        public static readonly string JobDataEntryTable = "(id int primary key auto_increment, " +
            "JobId varchar(128), Make varchar(128), Model varchar(128), Complaint varchar(512), Problem varchar(256)," +
            "ComplaintGroupings varchar(128), ProblemGroupings varchar(128), Requirements varchar(1024), Year int);";
        public static readonly string UserForumEntryTable = "(id int primary key auto_increment, MappedText varchar(512), UserId int);";
        public static readonly string GroupDefinitionTable = "(id int primary key auto_increment, GroupDefinition varchar(128));";
        public static readonly string CompanyIdTable = "(id int primary key auto_increment, LegalName varchar(256), ModelAccuracy float);";
        public static readonly string PartCatalogueTable = "(id int primary key auto_increment, Make varchar(128), Model varchar(128), Year int, PartId varchar(128), PartName varchar(194));";
        public static readonly string OverallUserTable = "(id int primary key auto_increment, " +
            "AccessLevel int, DerivedSecurityToken varbinary(64), SecurityQuestion varchar(256), PersonalData varbinary(1024), Settings varchar(512), Company int, " +
            "AuthToken varbinary(64), LoggedToken varchar(512), Job1Id varchar(128), Email varchar(128), RequestHistory varbinary("+RequestHistoryBytesSize+"), Job2Id varchar(128), Job1Results varbinary(2048), Job2Results varbinary(2048));";
        public static readonly string CompanySettings = "(id int primary key auto_increment, SettingKey varchar(64), SettingValue varchar(128));";
        public static readonly string PartsListAdditionRequest = "(id int primary key auto_increment, UserId int, ValidatedDataId int, RequestedAdditions varchar(256));";
        public static readonly string SafetyAdditionRequest = "(id int primary key auto_increment, UserId int, ValidatedDataId int, RequestedAdditions varchar(2048));";
        public static readonly string CompanyPartsRequest = "(id int primary key auto_increment, UserId int, JobId varchar(128), ReferencedParts varchar(256));";
        public static readonly string CompanyJoinRequest = "(id int primary key auto_increment, UserId int);";
    }
}
