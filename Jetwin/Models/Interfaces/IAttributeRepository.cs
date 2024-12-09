using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface IAttributeRepository
    {
        DataTable SearchAttributes(string searchInput);
        DataTable GetAttributes();
        DataTable GetAttributeTypes();
        DataTable GetAttributeValues(int attributeTypeID);
        bool IsAttributeTypeExists(string attributeType);
        bool IsAttributeValueExists(int attributeTypeID, string attributeValue);
        bool SaveAttributeType(string attributeType);
        bool SaveAttributeValue(int attributeTypeID, string attributeValue);
    }
}
