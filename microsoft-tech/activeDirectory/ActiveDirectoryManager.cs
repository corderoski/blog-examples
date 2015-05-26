using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text;

namespace TestAD.ActiveDirectory
{
    /*
     * 
     * MSDN: http://msdn.microsoft.com/en-us/library/system.directoryservices.accountmanagement%28v=vs.110%29.aspx
     */
    public class ActiveDirectoryManager
    {

        public const String DOMAIN_NAME = "DOMAINSRV";
        public const String LOCAL_DOMAIN = "@organization.local";
        public const String DOMAIN_ADDRESS = "organization.com";
        public const String OFFICE_NAME = "OrgAnizaTion";
        private const string TemporalPassword = "temporal2014";


        /// <summary>
        /// Creates an empty UserPrincipal object joined to the Domain.
        /// </summary>
        /// <returns></returns>
        protected static UserPrincipal Create()
        {
            var context = new PrincipalContext(ContextType.Domain, DOMAIN_NAME);
            var userObj = new UserPrincipal(context) { Enabled = true, PasswordNeverExpires = false };
            return userObj;
        }

        public static string CreateEmail(String user)
        {
            return String.Format("{0}@{1}", user.ToLowerInvariant(), DOMAIN_ADDRESS);
        }

        public static bool Disable(string userName)
        {
            var context = new PrincipalContext(ContextType.Domain, DOMAIN_NAME);
            var user = UserPrincipal.FindByIdentity(context, userName);
            if (user == null) return false;

            user.Enabled = false;
            user.Save();
            return true;
        }

        /// <summary>
        /// Checks if exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool Exists(String userName)
        {
            var context = new PrincipalContext(ContextType.Domain, DOMAIN_NAME);
            var userObj = UserPrincipal.FindByIdentity(context, userName);

            //  if not null, exists
            return userObj != null;
        }

        public static IEnumerable<string> FilterNonExist(IEnumerable<string> users)
        {
            return users.Where(userName => !Exists(userName)).ToList();
        }

        /// <summary>
        /// Checks if exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static UserPrincipal GetUserPrincipal(String userName)
        {
            var context = new PrincipalContext(ContextType.Domain, DOMAIN_NAME);
            var userObj = new UserPrincipal(context)
            {
                SamAccountName = userName
            };

            var searcher = new PrincipalSearcher
            {
                QueryFilter = userObj
            };

            return searcher.FindOne() as UserPrincipal;
        }

        /// <summary>
        /// Looks for an User using the Domain as datasource
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static User GetUserByUserName(String userName)
        {
            var dirSearch = new DirectorySearcher(GetDirectoryEntry);
            dirSearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";
            SearchResult results = dirSearch.FindOne();

            if (results != null)
            {
                var tempUser = new DirectoryEntry(results.Path);
                return ToUser(tempUser);

            }
            return null;
        }

        /// <summary>
        /// Looks for an User using the Domain as datasource
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A matched and filled User, otherwise, null.</returns>
        public static User GetUserByEmployeeId(String id)
        {
            var dirSearch = new DirectorySearcher(GetDirectoryEntry);
            dirSearch.Filter = "(&(objectClass=user)(|(ipPhone=*" + id + ")(pager=*" + id + ")))";
            var results = dirSearch.FindOne();

            if (results != null)
            {
                var tempUser = new DirectoryEntry(results.Path);
                return ToUser(tempUser);

            }
            return null;
        }

        /// <summary>
        /// Looks for an element by any of his fields.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static IEnumerable<User> GetUsers(string name, string userName, string email, string description)
        {
            if (String.IsNullOrEmpty(userName) && String.IsNullOrEmpty(name) && String.IsNullOrEmpty(email) && String.IsNullOrEmpty(description))
                return new User[0];

            var builder = new StringBuilder("(&(objectClass=user)(&");

            if (!String.IsNullOrEmpty(userName))
                builder.AppendFormat("({0}={1})", ADUserProperty.LOGINNAME, userName);
            if (!String.IsNullOrEmpty(name))
                builder.AppendFormat("({0}=*{1}*)", ADUserProperty.NAME, name);
            if (!String.IsNullOrEmpty(email))
                builder.AppendFormat("({0}={1})", ADUserProperty.EMAILADDRESS, email);
            if (!String.IsNullOrEmpty(description))
                builder.AppendFormat("({0}=*{1}*)", ADUserProperty.DESCRIPTION, description);

            //builder.AppendFormat("({0}={1})", ADUserProperty.USERACCOUNTCONTROL, 512);    //¿?

            builder.Append("))");

            var dirSearch = new DirectorySearcher(GetDirectoryEntry) { Filter = builder.ToString() };
            var results = dirSearch.FindAll();

            return results.Count > 0 ? 
                results.OfType<SearchResult>().Select(i => ToUser(i.GetDirectoryEntry())) : new User[0];
        }

        /// <summary>
        /// Exceptions:
        ///   System.InvalidOperationException:
        ///     The principal has not yet been associated with a System.DirectoryServices.AccountManagement.PrincipalContext
        ///     object.This type of principal cannot be inserted in the store.
        ///
        ///   System.DirectoryServices.AccountManagement.PrincipalOperationException:
        ///     An exception occurred when saving changes to the store, or updating the group
        ///     membership in the store.
        ///
        ///   System.DirectoryServices.AccountManagement.PrincipalExistsException:
        ///     The principal already occurs in the store.
        ///
        ///   System.DirectoryServices.AccountManagement.PasswordException:
        ///     The password does not meet complexity requirements.
        /// </summary>
        /// <param name="user"></param>
        protected static void Save(UserPrincipal user)
        {
            user.ExpirePasswordNow();
            user.Save();
        }

        public static void Save(User user, string password = TemporalPassword)
        {
			//This is a easy way to create an User.
            using (var userPrincipal = Create())
            {
                userPrincipal.DisplayName = user.DisplayName;
                userPrincipal.EmailAddress = user.EMail;
                userPrincipal.VoiceTelephoneNumber = user.Phone;
                /*
                userPrincipal.GivenName = user.Name;
                userPrincipal.MiddleName = user.LastName;
                userPrincipal.Description = user.Description;
                */
                userPrincipal.UserPrincipalName = user.UserName + LOCAL_DOMAIN;
                userPrincipal.SamAccountName = user.UserName;

                userPrincipal.Enabled = true;
                Save(userPrincipal);
            }
			// I wait, since sometimes the Active Directory/Server doesn't commit inmediatly.
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();

            /*
             *  Update Properties
             */

            var dirSearch = new DirectorySearcher(GetDirectoryEntry);
            dirSearch.Filter = "(&(objectClass=user)(" + ADUserProperty.LOGINNAME + "=" + user.UserName + "))";
            var results = dirSearch.FindOne();

            if (results == null) return;

            var entry = new DirectoryEntry(results.Path);

            entry.Invoke("SetPassword", password);
            entry.Properties[ADUserProperty.PAGER].Value = user.EmployeeId.ToString();
            entry.Properties[ADUserProperty.FIRSTNAME].Value = user.Name;
            entry.Properties[ADUserProperty.LASTNAME].Value = user.LastName;
            entry.Properties[ADUserProperty.DESCRIPTION].Value = user.Description;
            entry.Properties[ADUserProperty.EMAILADDRESS].Value = user.EMail;
            entry.Properties[ADUserProperty.HOMEPHONE].Value = user.Phone;

			// This makes the user change the password at the next login
            entry.Properties[ADUserProperty.PWDLASTSET].Value = 0;

            entry.Properties[ADUserProperty.TITLE].Value = user.Description;
            entry.Properties[ADUserProperty.DEPARTMENT].Value = user.Department;
            entry.Properties[ADUserProperty.COMPANY].Value = OFFICE_NAME;
            //entry.Properties[ADUserProperty.MANAGER].Value = String.Format("CN={0},CN=Users,DC=organization,DC=local", user.Supervisor.Name);

			// Attributes (codes) http://www.selfadsi.org/ads-attributes/user-userAccountControl.htm
            var val = (int)entry.Properties[ADUserProperty.USERACCOUNTCONTROL].Value;
            entry.Properties[ADUserProperty.USERACCOUNTCONTROL].Value = val & ~0x2;                  // (512 - Active)

            entry.CommitChanges();
            entry.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Object containing basic data.</returns>
        public static Manager GetManagerByEmpId(int id)
        {
            var user = GetUserByEmployeeId(id + "");
            return user != null ? new Manager() { EmployeeId = id, Name = user.DisplayName } : new Manager();
        }

        private static DirectoryEntry GetDirectoryEntry
        {
            get
            {
                var dirEntry = new DirectoryEntry();
                return dirEntry;
            }
        }

        private static User ToUser(Principal obj)
        {
            var dirSearch = new DirectorySearcher(GetDirectoryEntry);
            dirSearch.Filter = "(&(objectClass=user)(" + ADUserProperty.LOGINNAME + "=" + obj.SamAccountName + "))";
            var results = dirSearch.FindOne();

            if (results != null)
            {
                var tempUser = new DirectoryEntry(results.Path);
                return ToUser(tempUser);
            }
            return null;
        }

        private static User ToUser(DirectoryEntry directoryEntry)
        {
            var user = new User();
            user.UserName = GetProperty(directoryEntry, ADUserProperty.LOGINNAME);
            {
                var pager = GetProperty(directoryEntry, ADUserProperty.PAGER);
                var ipPhone = GetProperty(directoryEntry, ADUserProperty.EXTENSION);
                var empId = String.IsNullOrEmpty(pager) ? ipPhone : pager;
                //In my case, the organization uses theses fields for store personal data
                empId = empId.Replace("ID", "").Replace(" ", "");
                try
                {
                    user.EmployeeId = Int64.Parse(empId);
                }
                catch { }
            }
            user.DisplayName = GetProperty(directoryEntry, ADUserProperty.DISPLAYNAME);
            user.Name = GetProperty(directoryEntry, ADUserProperty.FIRSTNAME);
            user.LastName = GetProperty(directoryEntry, ADUserProperty.LASTNAME);
            user.Description = GetProperty(directoryEntry, ADUserProperty.DESCRIPTION);
            user.EMail = GetProperty(directoryEntry, ADUserProperty.EMAILADDRESS);
            return user;
        }

        private static String GetProperty(DirectoryEntry userDetail, String propertyName)
        {
            return userDetail.Properties.Contains(propertyName) ? userDetail.Properties[propertyName][0].ToString() : String.Empty;
        }

        /// <summary>
        /// ActiveDirectory basic's rules or policy on the password.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true - if valid, otherwise, false.</returns>
        public static bool IsValidPassword(string text)
        {
            return text.Length > 6;
        }
    }
}
