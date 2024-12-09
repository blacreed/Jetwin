using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly DataAccessLayer _db;

        public SupplierRepository()
        {
            _db = new DataAccessLayer();
        }

        public DataTable SearchSuppliers(string searchInput)
        {
            string query = @"
                SELECT 
                    s.SupplierID AS [ID],
                    s.Supplier AS [Supplier],
                    ci.ContactPerson AS [Agent],
                    ci.ContactNum AS [Contact Number],
                    s.Remarks AS [Remarks],
                    CASE 
                        WHEN s.StatusID = 1 THEN 'Active'
                        WHEN s.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Supplier s
                JOIN SupplierContactInfo ci ON s.ContactID = ci.ContactID
                WHERE (@SearchInput IS NULL OR 
                       s.Supplier LIKE '%' + @SearchInput + '%' OR 
                       ci.ContactPerson LIKE '%' + @SearchInput + '%' OR 
                       ci.ContactNum LIKE '%' + @SearchInput + '%')
                ORDER BY s.SupplierID DESC";
            return _db.ExecuteQuery(query, new Dictionary<string, object> { { "@SearchInput", searchInput ?? (object)DBNull.Value } });
        }

        public bool SaveSupplier(string supplierName, string agentName, string contactNumber, string remarks)
        {
            string queryContact = @"
                INSERT INTO SupplierContactInfo (ContactPerson, ContactNum)
                VALUES (@AgentName, @ContactNum);
                SELECT SCOPE_IDENTITY();";
            var contactId = Convert.ToInt32(_db.ExecuteScalar(queryContact, new Dictionary<string, object>
            {
                { "@AgentName", agentName },
                { "@ContactNum", contactNumber }
            }));

            string querySupplier = @"
                INSERT INTO Supplier (Supplier, ContactID, Remarks, StatusID)
                VALUES (@SupplierName, @ContactID, @Remarks, 1)";

            return _db.ExecuteNonQuery(querySupplier, new Dictionary<string, object>
            {
                { "@SupplierName", supplierName },
                { "@ContactID", contactId },
                { "@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks }
            });
        }

        public bool UpdateSupplier(int supplierId, string supplierName, string agentName, string contactNumber, string remarks)
        {
            string queryContact = @"
                UPDATE SupplierContactInfo
                SET ContactPerson = @AgentName, ContactNum = @ContactNum
                WHERE ContactID = (SELECT ContactID FROM Supplier WHERE SupplierID = @SupplierId)";
            bool contactUpdated = _db.ExecuteNonQuery(queryContact, new Dictionary<string, object>
            {
                { "@AgentName", agentName },
                { "@ContactNum", contactNumber },
                { "@SupplierId", supplierId }
            });

            string querySupplier = @"
                UPDATE Supplier
                SET Supplier = @SupplierName, Remarks = @Remarks
                WHERE SupplierID = @SupplierId";
            bool supplierUpdated = _db.ExecuteNonQuery(querySupplier, new Dictionary<string, object>
            {
                { "@SupplierName", supplierName },
                { "@Remarks", string.IsNullOrEmpty(remarks) ? (object)DBNull.Value : remarks },
                { "@SupplierId", supplierId }
            });

            return contactUpdated && supplierUpdated;
        }

        public bool ArchiveSupplier(int supplierId)
        {
            string query = "UPDATE Supplier SET StatusID = 3 WHERE SupplierID = @SupplierId";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@SupplierId", supplierId } });
        }

        public bool UnarchiveSupplier(int supplierId)
        {
            string query = "UPDATE Supplier SET StatusID = 1 WHERE SupplierID = @SupplierId";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@SupplierId", supplierId } });
        }
        public DataTable GetActiveSuppliers()
        {
            string query = @"
                SELECT SupplierID, Supplier
                FROM Supplier
                WHERE StatusID = 1 -- Active suppliers
                ORDER BY Supplier ASC";

            return _db.ExecuteQuery(query, null);
        }
    }
}
