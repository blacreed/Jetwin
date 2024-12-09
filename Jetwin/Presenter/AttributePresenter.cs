using Jetwin.Models;
using Jetwin.Models.Interfaces;
using Jetwin.Views.Interfaces;
using System.Data;

namespace Jetwin.Presenter
{
    public class AttributePresenter //MAINTENANCE MODULE -> ATTRIBUTE SUBMODULE
    {
        private readonly IMaintenanceView _view;
        private readonly IAttributeRepository _repository;

        public AttributePresenter(IMaintenanceView view)
        {
            _view = view;
            _repository = new AttributeRepository();
        }

        public DataTable LoadAttributes()
        {
            return _repository.GetAttributes();
        }
        public void SearchAttributes(string searchInput)
        {
            var attributes = _repository.SearchAttributes(searchInput);
            _view.DisplayAttributes(attributes);
        }
        public void SaveAttributeType(string attributeType)
        {
            if (_repository.IsAttributeTypeExists(attributeType))
            {
                _view.ShowValidationMessage("Attribute type already exists.");
                return;
            }

            if (_repository.SaveAttributeType(attributeType))
            {
                _view.ShowValidationMessage("Attribute type saved successfully.");
                _view.PopulateAttributeTypes(_repository.GetAttributeTypes());
                _view.DisplayAttributes(_repository.GetAttributes());
            }
            else
            {
                _view.ShowValidationMessage("Failed to save attribute type.");
            }
        }

        public void SaveAttributeValue(int attributeTypeID, string attributeValue)
        {
            if (_repository.IsAttributeValueExists(attributeTypeID, attributeValue))
            {
                _view.ShowValidationMessage("Attribute value already exists.");
                return;
            }

            if (_repository.SaveAttributeValue(attributeTypeID, attributeValue))
            {
                _view.ShowValidationMessage("Attribute value saved successfully.");
                _view.PopulateAttributeValues(_repository.GetAttributeValues(attributeTypeID));
                _view.DisplayAttributes(_repository.GetAttributes());
            }
            else
            {
                _view.ShowValidationMessage("Failed to save attribute value.");
            }
        }
    }
}
