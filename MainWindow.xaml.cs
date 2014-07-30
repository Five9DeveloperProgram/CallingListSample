/* 
Copyright (C) 2014 Five9, Inc. All rights reserved.

www.five9.com/legal#terms

Software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES 
 OR CONDITIONS OF ANY KIND, either express or implied.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Five9CommonLib;
using CallingListSample.Five9Admin;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;

namespace CallingListSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WsAdminV2Client _adminClient = null;
        private string _csvData =
            "Number,FirstName,LastName\r\n" +
            "8885551111,John,Smith1\r\n" +
            "8885551112,John,Smith2\r\n" +
            "8885551113,John,Smith3\r\n" +
            "8885551114,John,Smith4\r\n" +
            "8885551115,John,Smith5\r\n";

        public MainWindow()
        {
            InitializeComponent();

            _adminClient = new WsAdminV2Client();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // Add our AuthHeaderInserter behavior to the client endpoint
            // this will invoke our behavior before every send so that
            // we can insert the "Authorization" HTTP header before it is sent.
            AuthHeaderInserter inserter = new AuthHeaderInserter();
            inserter.Username = tbxUsername.Text;
            inserter.Password = pbxPassword.Password;
            _adminClient.Endpoint.Behaviors.Add(new AuthHeaderBehavior(inserter));

            byte[] authbytes = Encoding.UTF8.GetBytes(string.Concat(tbxUsername.Text, ":", pbxPassword.Password));
            string base64 = Convert.ToBase64String(authbytes);
            this.tbxBase64Credentials.Text = string.Concat("Basic ", base64);

            getCallCountersState req = new getCallCountersState();
            limitTimeoutState[] limitTimeoutStates = _adminClient.getCallCountersState(req);

            LogLimitTimeoutStates(limitTimeoutStates);

            getListsInfo reqLists = new getListsInfo();
            reqLists.listNamePattern = ".*";
            listInfo[] lists = _adminClient.getListsInfo(reqLists);

            foreach (listInfo list in lists)
            {
                cbxListName.Items.Add(list.name);
            }

            MessageBox.Show("Connected!", "Connect", MessageBoxButton.OK);
        }

        private void btnAddRecordToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addRecordToList req = new addRecordToList();
                req.listName = cbxListName.Text;
                req.listUpdateSettings = new listUpdateSettings();

                req.listUpdateSettings.allowDataCleanupSpecified = false;
                req.listUpdateSettings.cleanListBeforeUpdate = false;

                req.listUpdateSettings.callNowMode = callNowMode.NONE;
                req.listUpdateSettings.callNowModeSpecified = true;

                req.listUpdateSettings.crmAddMode = crmAddMode.ADD_NEW;
                req.listUpdateSettings.crmAddModeSpecified = true;

                req.listUpdateSettings.crmUpdateMode = crmUpdateMode.UPDATE_FIRST;
                req.listUpdateSettings.crmUpdateModeSpecified = true;

                req.listUpdateSettings.listAddMode = listAddMode.ADD_FIRST;
                req.listUpdateSettings.listAddModeSpecified = true;

                req.listUpdateSettings.fieldsMapping = new fieldEntry[3];

                req.listUpdateSettings.fieldsMapping[0] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[0].columnNumber = 1;
                req.listUpdateSettings.fieldsMapping[0].fieldName = "number1";
                req.listUpdateSettings.fieldsMapping[0].key = true;

                req.listUpdateSettings.fieldsMapping[1] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[1].columnNumber = 2;
                req.listUpdateSettings.fieldsMapping[1].fieldName = "first_name";
                req.listUpdateSettings.fieldsMapping[1].key = false;

                req.listUpdateSettings.fieldsMapping[2] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[2].columnNumber = 3;
                req.listUpdateSettings.fieldsMapping[2].fieldName = "last_name";
                req.listUpdateSettings.fieldsMapping[2].key = false;

                string[][] data = buildImportData();

                req.record = data[0];

                addRecordToListResponse resp = null;

                resp = _adminClient.addRecordToList(req);
                ShowListImportResult(resp.@return);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnDeleteRecordFromList_Click(object sender, RoutedEventArgs e)
        {
            deleteRecordFromList req = new deleteRecordFromList();
            req.listName = cbxListName.Text;
            req.listDeleteSettings = new listDeleteSettings();

            req.listDeleteSettings.allowDataCleanupSpecified = false;

            req.listDeleteSettings.listDeleteMode = listDeleteMode.DELETE_IF_SOLE_CRM_MATCH;
            req.listDeleteSettings.listDeleteModeSpecified = true;

            req.listDeleteSettings.fieldsMapping = new fieldEntry[3];

            req.listDeleteSettings.fieldsMapping[0] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[0].columnNumber = 1;
            req.listDeleteSettings.fieldsMapping[0].fieldName = "number1";
            req.listDeleteSettings.fieldsMapping[0].key = true;

            req.listDeleteSettings.fieldsMapping[1] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[1].columnNumber = 2;
            req.listDeleteSettings.fieldsMapping[1].fieldName = "first_name";
            req.listDeleteSettings.fieldsMapping[1].key = false;

            req.listDeleteSettings.fieldsMapping[2] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[2].columnNumber = 3;
            req.listDeleteSettings.fieldsMapping[2].fieldName = "last_name";
            req.listDeleteSettings.fieldsMapping[2].key = false;

            string[][] data = buildImportData();

            req.record = data[0];

            deleteRecordFromListResponse resp = null;

            resp = _adminClient.deleteRecordFromList(req);
            ShowListImportResult(resp.@return);
        }

        private void btnAddToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addToList req = new addToList();
                req.listName = cbxListName.Text;
                req.listUpdateSettings = new listUpdateSettings();

                req.listUpdateSettings.allowDataCleanupSpecified = false;
                req.listUpdateSettings.cleanListBeforeUpdate = false;

                req.listUpdateSettings.callNowMode = callNowMode.NONE;
                req.listUpdateSettings.callNowModeSpecified = true;

                req.listUpdateSettings.crmAddMode = crmAddMode.ADD_NEW;
                req.listUpdateSettings.crmAddModeSpecified = true;

                req.listUpdateSettings.crmUpdateMode = crmUpdateMode.UPDATE_FIRST;
                req.listUpdateSettings.crmUpdateModeSpecified = true;

                req.listUpdateSettings.listAddMode = listAddMode.ADD_FIRST;
                req.listUpdateSettings.listAddModeSpecified = true;

                req.listUpdateSettings.fieldsMapping = new fieldEntry[3];

                req.listUpdateSettings.fieldsMapping[0] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[0].columnNumber = 1;
                req.listUpdateSettings.fieldsMapping[0].fieldName = "number1";
                req.listUpdateSettings.fieldsMapping[0].key = true;

                req.listUpdateSettings.fieldsMapping[1] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[1].columnNumber = 2;
                req.listUpdateSettings.fieldsMapping[1].fieldName = "first_name";
                req.listUpdateSettings.fieldsMapping[1].key = false;

                req.listUpdateSettings.fieldsMapping[2] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[2].columnNumber = 3;
                req.listUpdateSettings.fieldsMapping[2].fieldName = "last_name";
                req.listUpdateSettings.fieldsMapping[2].key = false;

                req.importData = buildImportData();

                addToListResponse resp = null;

                resp = _adminClient.addToList(req);

                MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

                string jobId = resp.@return.identifier;

                bool done = false;
                while (!done)
                {
                    isImportRunning reqRunning = new isImportRunning();
                    reqRunning.identifier = new importIdentifier();
                    reqRunning.identifier.identifier = jobId;
                    reqRunning.waitTime = 15;
                    reqRunning.waitTimeSpecified = true;

                    isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                    done = !respRunning.@return;
                }

                getListImportResult reqResults = new getListImportResult();
                reqResults.identifier = new importIdentifier();
                reqResults.identifier.identifier = resp.@return.identifier;
                getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

                ShowListImportResult(respResults.@return);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnDeleteFromList_Click(object sender, RoutedEventArgs e)
        {
            deleteFromList req = new deleteFromList();
            req.listName = cbxListName.Text;
            req.listDeleteSettings = new listDeleteSettings();

            req.listDeleteSettings.allowDataCleanupSpecified = false;

            req.listDeleteSettings.listDeleteMode = listDeleteMode.DELETE_IF_SOLE_CRM_MATCH;
            req.listDeleteSettings.listDeleteModeSpecified = true;

            req.listDeleteSettings.fieldsMapping = new fieldEntry[3];

            req.listDeleteSettings.fieldsMapping[0] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[0].columnNumber = 1;
            req.listDeleteSettings.fieldsMapping[0].fieldName = "number1";
            req.listDeleteSettings.fieldsMapping[0].key = true;

            req.listDeleteSettings.fieldsMapping[1] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[1].columnNumber = 2;
            req.listDeleteSettings.fieldsMapping[1].fieldName = "first_name";
            req.listDeleteSettings.fieldsMapping[1].key = false;

            req.listDeleteSettings.fieldsMapping[2] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[2].columnNumber = 3;
            req.listDeleteSettings.fieldsMapping[2].fieldName = "last_name";
            req.listDeleteSettings.fieldsMapping[2].key = false;

            req.importData = buildImportData();

            deleteFromListResponse resp = null;

            resp = _adminClient.deleteFromList(req);

            MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

            string jobId = resp.@return.identifier;

            bool done = false;
            while (!done)
            {
                isImportRunning reqRunning = new isImportRunning();
                reqRunning.identifier = new importIdentifier();
                reqRunning.identifier.identifier = jobId;
                reqRunning.waitTime = 15;
                reqRunning.waitTimeSpecified = true;

                isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                done = !respRunning.@return;
            }

            getListImportResult reqResults = new getListImportResult();
            reqResults.identifier = new importIdentifier();
            reqResults.identifier.identifier = resp.@return.identifier;
            getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

            ShowListImportResult(respResults.@return);
        }

        private void btnAddToListCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addToListCsv req = new addToListCsv();
                req.listName = cbxListName.Text;
                req.listUpdateSettings = new listUpdateSettings();

                req.listUpdateSettings.allowDataCleanupSpecified = false;
                req.listUpdateSettings.cleanListBeforeUpdate = false;

                req.listUpdateSettings.callNowMode = callNowMode.NONE;
                req.listUpdateSettings.callNowModeSpecified = true;

                req.listUpdateSettings.crmAddMode = crmAddMode.ADD_NEW;
                req.listUpdateSettings.crmAddModeSpecified = true;

                req.listUpdateSettings.crmUpdateMode = crmUpdateMode.UPDATE_FIRST;
                req.listUpdateSettings.crmUpdateModeSpecified = true;

                req.listUpdateSettings.listAddMode = listAddMode.ADD_FIRST;
                req.listUpdateSettings.listAddModeSpecified = true;

                req.listUpdateSettings.fieldsMapping = new fieldEntry[3];

                req.listUpdateSettings.fieldsMapping[0] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[0].columnNumber = 1;
                req.listUpdateSettings.fieldsMapping[0].fieldName = "number1";
                req.listUpdateSettings.fieldsMapping[0].key = true;

                req.listUpdateSettings.fieldsMapping[1] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[1].columnNumber = 2;
                req.listUpdateSettings.fieldsMapping[1].fieldName = "first_name";
                req.listUpdateSettings.fieldsMapping[1].key = false;

                req.listUpdateSettings.fieldsMapping[2] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[2].columnNumber = 3;
                req.listUpdateSettings.fieldsMapping[2].fieldName = "last_name";
                req.listUpdateSettings.fieldsMapping[2].key = false;

                req.csvData = this._csvData;

                addToListCsvResponse resp = null;

                resp = _adminClient.addToListCsv(req);

                MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

                string jobId = resp.@return.identifier;

                bool done = false;
                while (!done)
                {
                    isImportRunning reqRunning = new isImportRunning();
                    reqRunning.identifier = new importIdentifier();
                    reqRunning.identifier.identifier = jobId;
                    reqRunning.waitTime = 15;
                    reqRunning.waitTimeSpecified = true;

                    isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                    done = !respRunning.@return;
                }

                getListImportResult reqResults = new getListImportResult();
                reqResults.identifier = new importIdentifier();
                reqResults.identifier.identifier = resp.@return.identifier;
                getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

                ShowListImportResult(respResults.@return);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnDeleteFromListCsv_Click(object sender, RoutedEventArgs e)
        {
            deleteFromListCsv req = new deleteFromListCsv();
            req.listName = cbxListName.Text;
            req.listDeleteSettings = new listDeleteSettings();

            req.listDeleteSettings.allowDataCleanupSpecified = false;

            req.listDeleteSettings.listDeleteMode = listDeleteMode.DELETE_IF_SOLE_CRM_MATCH;
            req.listDeleteSettings.listDeleteModeSpecified = true;

            req.listDeleteSettings.fieldsMapping = new fieldEntry[3];

            req.listDeleteSettings.fieldsMapping[0] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[0].columnNumber = 1;
            req.listDeleteSettings.fieldsMapping[0].fieldName = "number1";
            req.listDeleteSettings.fieldsMapping[0].key = true;

            req.listDeleteSettings.fieldsMapping[1] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[1].columnNumber = 2;
            req.listDeleteSettings.fieldsMapping[1].fieldName = "first_name";
            req.listDeleteSettings.fieldsMapping[1].key = false;

            req.listDeleteSettings.fieldsMapping[2] = new fieldEntry();
            req.listDeleteSettings.fieldsMapping[2].columnNumber = 3;
            req.listDeleteSettings.fieldsMapping[2].fieldName = "last_name";
            req.listDeleteSettings.fieldsMapping[2].key = false;

            req.csvData = this._csvData;

            deleteFromListCsvResponse resp = null;

            resp = _adminClient.deleteFromListCsv(req);

            MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

            string jobId = resp.@return.identifier;

            bool done = false;
            while (!done)
            {
                isImportRunning reqRunning = new isImportRunning();
                reqRunning.identifier = new importIdentifier();
                reqRunning.identifier.identifier = jobId;
                reqRunning.waitTime = 15;
                reqRunning.waitTimeSpecified = true;

                isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                done = !respRunning.@return;
            }

            getListImportResult reqResults = new getListImportResult();
            reqResults.identifier = new importIdentifier();
            reqResults.identifier.identifier = resp.@return.identifier;
            getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

            ShowListImportResult(respResults.@return);
        }

        private void btnAddToListFTP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addToListFtp req = new addToListFtp();
                req.listName = cbxListName.Text;
                req.updateSettings = new listUpdateSettings();

                req.updateSettings.allowDataCleanupSpecified = false;
                req.updateSettings.cleanListBeforeUpdate = false;

                req.updateSettings.callNowMode = callNowMode.NONE;
                req.updateSettings.callNowModeSpecified = true;

                req.updateSettings.crmAddMode = crmAddMode.ADD_NEW;
                req.updateSettings.crmAddModeSpecified = true;

                req.updateSettings.crmUpdateMode = crmUpdateMode.UPDATE_FIRST;
                req.updateSettings.crmUpdateModeSpecified = true;

                req.updateSettings.listAddMode = listAddMode.ADD_FIRST;
                req.updateSettings.listAddModeSpecified = true;

                req.updateSettings.fieldsMapping = new fieldEntry[3];

                req.updateSettings.fieldsMapping[0] = new fieldEntry();
                req.updateSettings.fieldsMapping[0].columnNumber = 1;
                req.updateSettings.fieldsMapping[0].fieldName = "number1";
                req.updateSettings.fieldsMapping[0].key = true;

                req.updateSettings.fieldsMapping[1] = new fieldEntry();
                req.updateSettings.fieldsMapping[1].columnNumber = 2;
                req.updateSettings.fieldsMapping[1].fieldName = "first_name";
                req.updateSettings.fieldsMapping[1].key = false;

                req.updateSettings.fieldsMapping[2] = new fieldEntry();
                req.updateSettings.fieldsMapping[2].columnNumber = 3;
                req.updateSettings.fieldsMapping[2].fieldName = "last_name";
                req.updateSettings.fieldsMapping[2].key = false;

                req.ftpSettings = new ftpImportSettings();
                req.ftpSettings.hostname = "http://www.mycompany.com";
                req.ftpSettings.path = "/listdata/myfile.csv";
                req.ftpSettings.username = "username";
                req.ftpSettings.password = "password";

                MessageBox.Show("Please change FTP credentials and make sure file exists, then remove this message.");

                //addToListFtpResponse resp = _adminClient.addToListFtp(req);
                //MessageBox.Show("FTP Upload Submitted");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnDeleteFromListFtp_Click(object sender, RoutedEventArgs e)
        {
            deleteFromListFtp req = new deleteFromListFtp();
            req.listName = cbxListName.Text;
            req.deleteSettings = new listDeleteSettings();

            req.deleteSettings.allowDataCleanupSpecified = false;

            req.deleteSettings.listDeleteMode = listDeleteMode.DELETE_IF_SOLE_CRM_MATCH;
            req.deleteSettings.listDeleteModeSpecified = true;

            req.deleteSettings.fieldsMapping = new fieldEntry[3];

            req.deleteSettings.fieldsMapping[0] = new fieldEntry();
            req.deleteSettings.fieldsMapping[0].columnNumber = 1;
            req.deleteSettings.fieldsMapping[0].fieldName = "number1";
            req.deleteSettings.fieldsMapping[0].key = true;

            req.deleteSettings.fieldsMapping[1] = new fieldEntry();
            req.deleteSettings.fieldsMapping[1].columnNumber = 2;
            req.deleteSettings.fieldsMapping[1].fieldName = "first_name";
            req.deleteSettings.fieldsMapping[1].key = false;

            req.deleteSettings.fieldsMapping[2] = new fieldEntry();
            req.deleteSettings.fieldsMapping[2].columnNumber = 3;
            req.deleteSettings.fieldsMapping[2].fieldName = "last_name";
            req.deleteSettings.fieldsMapping[2].key = false;

            req.ftpSettings = new ftpImportSettings();
            req.ftpSettings.hostname = "http://www.mycompany.com";
            req.ftpSettings.path = "/listdata/myfile.csv";
            req.ftpSettings.username = "username";
            req.ftpSettings.password = "password";

            MessageBox.Show("Please change FTP credentials and make sure file exists, then remove this message.");

            //deleteFromListFtpResponse resp = _adminClient.deleteFromListFtp(req);
            //MessageBox.Show("FTP Upload Submitted");
        }

        private void btnAsyncAddRecordsToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                asyncAddRecordsToList req = new asyncAddRecordsToList();
                req.listName = cbxListName.Text;
                req.listUpdateSettings = new listUpdateSettings();

                req.listUpdateSettings.allowDataCleanupSpecified = false;
                req.listUpdateSettings.cleanListBeforeUpdate = false;

                req.listUpdateSettings.callNowMode = callNowMode.NONE;
                req.listUpdateSettings.callNowModeSpecified = true;

                req.listUpdateSettings.crmAddMode = crmAddMode.ADD_NEW;
                req.listUpdateSettings.crmAddModeSpecified = true;

                req.listUpdateSettings.crmUpdateMode = crmUpdateMode.UPDATE_FIRST;
                req.listUpdateSettings.crmUpdateModeSpecified = true;

                req.listUpdateSettings.listAddMode = listAddMode.ADD_FIRST;
                req.listUpdateSettings.listAddModeSpecified = true;

                req.listUpdateSettings.fieldsMapping = new fieldEntry[3];

                req.listUpdateSettings.fieldsMapping[0] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[0].columnNumber = 1;
                req.listUpdateSettings.fieldsMapping[0].fieldName = "number1";
                req.listUpdateSettings.fieldsMapping[0].key = true;

                req.listUpdateSettings.fieldsMapping[1] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[1].columnNumber = 2;
                req.listUpdateSettings.fieldsMapping[1].fieldName = "first_name";
                req.listUpdateSettings.fieldsMapping[1].key = false;

                req.listUpdateSettings.fieldsMapping[2] = new fieldEntry();
                req.listUpdateSettings.fieldsMapping[2].columnNumber = 3;
                req.listUpdateSettings.fieldsMapping[2].fieldName = "last_name";
                req.listUpdateSettings.fieldsMapping[2].key = false;

                req.importData = buildImportData();

                asyncAddRecordsToListResponse resp = _adminClient.asyncAddRecordsToList(req);

                MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

                string jobId = resp.@return.identifier;

                bool done = false;
                while (!done) 
                {
                    isImportRunning reqRunning = new isImportRunning();
                    reqRunning.identifier = new importIdentifier();
                    reqRunning.identifier.identifier = jobId;
                    reqRunning.waitTime = 15;
                    reqRunning.waitTimeSpecified = true;

                    isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                    done = !respRunning.@return;

                }

                getListImportResult reqResults = new getListImportResult();
                reqResults.identifier = new importIdentifier();
                reqResults.identifier.identifier = jobId;
                getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

                ShowListImportResult(respResults.@return);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }
        }

        private void btnAsyncDeleteRecordsFromList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                asyncDeleteRecordsFromList req = new asyncDeleteRecordsFromList();
                req.listName = cbxListName.Text;
                req.listDeleteSettings = new listDeleteSettings();

                req.listDeleteSettings.allowDataCleanupSpecified = false;

                req.listDeleteSettings.listDeleteMode = listDeleteMode.DELETE_IF_SOLE_CRM_MATCH;
                req.listDeleteSettings.listDeleteModeSpecified = true;

                req.listDeleteSettings.fieldsMapping = new fieldEntry[3];

                req.listDeleteSettings.fieldsMapping[0] = new fieldEntry();
                req.listDeleteSettings.fieldsMapping[0].columnNumber = 1;
                req.listDeleteSettings.fieldsMapping[0].fieldName = "number1";
                req.listDeleteSettings.fieldsMapping[0].key = true;

                req.listDeleteSettings.fieldsMapping[1] = new fieldEntry();
                req.listDeleteSettings.fieldsMapping[1].columnNumber = 2;
                req.listDeleteSettings.fieldsMapping[1].fieldName = "first_name";
                req.listDeleteSettings.fieldsMapping[1].key = false;

                req.listDeleteSettings.fieldsMapping[2] = new fieldEntry();
                req.listDeleteSettings.fieldsMapping[2].columnNumber = 3;
                req.listDeleteSettings.fieldsMapping[2].fieldName = "last_name";
                req.listDeleteSettings.fieldsMapping[2].key = false;

                req.importData = buildImportData();

                asyncDeleteRecordsFromListResponse resp = _adminClient.asyncDeleteRecordsFromList(req);

                MessageBox.Show("Operation submitted.  Click OK to wait for results.", "Operation", MessageBoxButton.OK);

                string jobId = resp.@return.identifier;

                bool done = false;
                while (!done)
                {
                    isImportRunning reqRunning = new isImportRunning();
                    reqRunning.identifier = new importIdentifier();
                    reqRunning.identifier.identifier = jobId;
                    reqRunning.waitTime = 15;
                    reqRunning.waitTimeSpecified = true;

                    isImportRunningResponse respRunning = _adminClient.isImportRunning(reqRunning);
                    done = !respRunning.@return;
                }

                getListImportResult reqResults = new getListImportResult();
                reqResults.identifier = new importIdentifier();
                reqResults.identifier.identifier = jobId;
                getListImportResultResponse respResults = _adminClient.getListImportResult(reqResults);

                ShowListImportResult(respResults.@return);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                return;
            }
        }

        public string[][] buildImportData()
        {
            ArrayList al = new ArrayList();

            string exp = "(\\S*)$*";

            Match match = Regex.Match(_csvData, exp);

            while (match.Success)
            {
                Debug.WriteLine("" + match.Index + " - " + match.Length + " - " + match.Value);

                if (match.Length > 1)
                {
                    Debug.WriteLine(match.Value);
                    al.Add(match.Value);
                }

                match = match.NextMatch();
            }

            // We need to manually skip the header record here for the async operations.
            string[][] ret = new string[al.Count - 1][];

            for( int i = 0; i < al.Count - 1; i++ )
            {
                string csv = (string)al[i + 1];

                ret[i] = csv.Split(",".ToCharArray());
            }

            return ret;
        }

        private void ShowListImportResult(listImportResult res)
        {
            string s = "";

            s += "List Name: " + res.listName + "\r\n";
            s += "Warning Count: " + res.warningsCount + "\r\n";
            s += "Failure Message: " + res.failureMessage + "\r\n";
            s += "---------------------------------\r\n";
            s += "Call Now Queued: " + res.callNowQueued + "\r\n";
            s += "CRM Records Inserted: " + res.crmRecordsInserted + "\r\n";
            s += "CRM Records Updated: " + res.crmRecordsUpdated + "\r\n";
            s += "---------------------------------\r\n";
            s += "List Records Inserted: " + res.listRecordsInserted + "\r\n";
            s += "List Records Deleted: " + res.listRecordsDeleted + "\r\n";
            s += "---------------------------------\r\n";
            s += "Upload Duplicates Count: " + res.uploadDuplicatesCount + "\r\n";
            s += "Upload Errors Count: " + res.uploadErrorsCount + "\r\n";

            MessageBox.Show(s, "List Import Result", MessageBoxButton.OK);
        }

        private void LogLimitTimeoutStates(limitTimeoutState[] limitTimeoutStates)
        {
            if (limitTimeoutStates != null)
            {
                Debug.WriteLine("Limit Timeout States");
                Debug.WriteLine("--------------------");
                foreach (limitTimeoutState lti in limitTimeoutStates)
                {
                    Debug.WriteLine("Timeout: " + lti.timeout);
                    foreach (callCounterState ccs in lti.callCounterStates)
                    {
                        Debug.WriteLine("    Operation: " + ccs.operationType + ", Limit[" + ccs.limit + "], Value[" + ccs.value + "]" + ((ccs.value >= ccs.limit) ? " ---> Exceeded" : ""));
                    }
                }
                Debug.WriteLine("--------------------");
            }
        }
    }
}
