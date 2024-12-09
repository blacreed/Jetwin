using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class AttributeRepository : IAttributeRepository
    {
        private readonly DataAccessLayer _db;

        public AttributeRepository()
        {
            _db = new DataAccessLayer();
        }
        public DataTable SearchAttributes(string searchInput)
        {
            string query = @"
                SELECT 
                    at.AttributeTypeName AS [Attribute Type],
                    av.AttributeValueName AS [Attribute Value],
                    CASE 
                        WHEN at.StatusID = 1 THEN 'Active'
                        WHEN at.StatusID = 2 THEN 'Inactive'
                        WHEN at.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Type Status],
                    CASE 
                        WHEN av.StatusID = 1 THEN 'Active'
                        WHEN av.StatusID = 2 THEN 'Inactive'
                        WHEN av.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Value Status]
                FROM AttributeType at
                LEFT JOIN AttributeValue av ON at.AttributeTypeID = av.AttributeTypeID
                WHERE (ISNULL(@SearchInput, '') = '' 
                       OR at.AttributeTypeName LIKE '%' + @SearchInput + '%' 
                       OR av.AttributeValueName LIKE '%' + @SearchInput + '%')
                ORDER BY at.AttributeTypeName, av.AttributeValueName;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
                }
        //get all attributes (type and values)
        public DataTable GetAttributes()
        {
            string query = @"
                SELECT 
                    at.AttributeTypeName AS [Attribute Type],
                    av.AttributeValueName AS [Attribute Value],
                    CASE 
                        WHEN at.StatusID = 1 THEN 'Active'
                        WHEN at.StatusID = 2 THEN 'Inactive'
                        WHEN at.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Type Status],
                    CASE 
                        WHEN av.StatusID = 1 THEN 'Active'
                        WHEN av.StatusID = 2 THEN 'Inactive'
                        WHEN av.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Value Status]
                FROM AttributeType at
                LEFT JOIN AttributeValue av ON at.AttributeTypeID = av.AttributeTypeID
                ORDER BY at.AttributeTypeName, av.AttributeValueName;";
            return _db.ExecuteQuery(query, null);
        }

        //get attribute types
        public DataTable GetAttributeTypes()
        {
            string query = @"
                SELECT 
                    AttributeTypeID, 
                    AttributeTypeName 
                FROM AttributeType
                WHERE StatusID = 1
                ORDER BY AttributeTypeID DESC";
            return _db.ExecuteQuery(query, null);
        }
        public bool SaveAttributeType(string attributeType)
        {
            string query = "INSERT INTO AttributeType (AttributeTypeName, StatusID) VALUES (@Name, 1)";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@Name", attributeType } });
        }
        //check if an attribute type exists
        public bool IsAttributeTypeExists(string attributeType)
        {
            string query = "SELECT COUNT(*) FROM AttributeType WHERE AttributeTypeName = @Name";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@Name", attributeType } }) > 0;
        }


        //get attribute values for a specific type
        public DataTable GetAttributeValues(int attributeTypeID)
        {
            string query = @"
                SELECT 
                    AttributeValueID, 
                    AttributeValueName 
                FROM AttributeValue
                WHERE AttributeTypeID = @TypeID AND StatusID = 1
                ORDER BY AttributeValueID DESC";
            return _db.ExecuteQuery(query, new Dictionary<string, object> { { "@TypeID", attributeTypeID } });
        }
        public bool SaveAttributeValue(int attributeTypeID, string attributeValue)
        {
            string query = "INSERT INTO AttributeValue (AttributeTypeID, AttributeValueName, StatusID) VALUES (@TypeID, @Value, 1)";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@TypeID", attributeTypeID },
                { "@Value", attributeValue }
            });
        }

        //check if an attribute value exists
        public bool IsAttributeValueExists(int attributeTypeID, string attributeValue)
        {
            string query = "SELECT COUNT(*) FROM AttributeValue WHERE AttributeTypeID = @TypeID AND AttributeValueName = @Value";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object>
        {
            { "@TypeID", attributeTypeID },
            { "@Value", attributeValue }
        }) > 0;
        }

    }
}
