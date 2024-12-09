using Jetwin.Models;
using Jetwin.Modules;
using Jetwin.Utility;

namespace Jetwin.Presenter
{
    public class UserPresenter //MAINTENANCE MODULE -> USER SUBMODULE
    {
        private readonly MaintenanceView _view;
        private readonly UserRepository _repository;

        public UserPresenter(MaintenanceView view)
        {
            _view = view;
            _repository = new UserRepository();
        }

        //get user data and display into data grid
        public void LoadUsers(string search = null)
        {
            var users = _repository.GetUsers(search);
            _view.DisplayUsers(users);
        }

        public void SaveUser()
        {
            var empName = _view.EmpName;
            var username = _view.Username;
            var password = _view.Password;
            var confirmPassword = _view.ConfirmPassword;
            var contactNumber = _view.ContactNumber;
            var editingId = _view.editingId;

            //if editing
            if (editingId.HasValue)
            {
                //validation check
                if (!InputValidator.ValidateUserInputs(empName, username, contactNumber, out string validationMessage))
                {
                    _view.ShowValidationMessage(validationMessage);
                    return;
                }
                if (_repository.IsDuplicateUser(editingId.Value, empName, username, contactNumber))
                {
                    _view.ShowValidationMessage("Duplicate Employee Name, Username, or Contact Number detected. Please use unique values.");
                    return;
                }
                //save edited user details
                if (_repository.UpdateUser(editingId.Value, empName, username, contactNumber))
                {
                    _view.ShowValidationMessage("User updated successfully.");
                    LoadUsers();
                    _view.ClearUserInputs();
                    _view.ResetEditState();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to update user.");
                }
            }
            //if not editing
            else
            {
                //validation checks
                if (!InputValidator.ValidateUserInputs(empName, username, password, confirmPassword, contactNumber, out string validationMessage))
                {
                    _view.ShowValidationMessage(validationMessage);
                    return;
                }
                if (_repository.IsDuplicateUser(empName, username, contactNumber))
                {
                    _view.ShowValidationMessage("Employee name already exists");
                    return;
                }
                if (_repository.IsUsernameExists(username))
                {
                    _view.ShowValidationMessage("Username already exists. Please choose another.");
                    return;
                }
                //save user details
                if (_repository.SaveUser(empName, username, password, contactNumber))
                {
                    _view.ShowValidationMessage("User saved successfully.");
                    LoadUsers();
                    _view.ClearUserInputs();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to save user.");
                }
            }
            
        }

        public void ArchiveUser(int userId)
        {
            if (_repository.ArchiveUser(userId))
            {
                _view.ShowValidationMessage("User archived successfully.");
                LoadUsers();
            }
            else
            {
                _view.ShowValidationMessage("Failed to archive user.");
            }

            
        }
        public void UnarchiveUser(int userId)
        {
            if (_repository.UnarchiveUser(userId))
            {
                _view.ShowValidationMessage("User unarchived successfully.");
                LoadUsers();
            }
            else
            {
                _view.ShowValidationMessage("Failed to unarchive user.");
            }
        }
    }
}
