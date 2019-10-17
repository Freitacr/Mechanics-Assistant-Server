﻿using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace MechanicsAssistantServer.Data.MySql.TableDataTypes
{
    class JoinRequest : ISqlSerializable
    {
        public static readonly TableDataManipulator<JoinRequest> Manipulator = new TableDataManipulator<JoinRequest>();
        public int UserId { get; set; }
        
        public JoinRequest()
        {

        }

        public JoinRequest(int userId)
        {
            UserId = userId;
        }
        
        public ISqlSerializable Copy()
        {
            return new JoinRequest(UserId);
        }

        public void Deserialize(MySqlDataReader reader)
        {
            UserId = (int)reader["UserId"];
        }

        public string Serialize(string tableName)
        {
            return "insert into " + tableName + "(UserId) values (" + UserId + ");";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return (obj as JoinRequest).UserId == UserId;
        }

        public override int GetHashCode()
        {
            return UserId;
        }
    }
}