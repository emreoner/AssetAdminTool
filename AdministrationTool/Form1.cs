using AdministrationTool.Controls;
using AdministrationTool.Extensions;
using AdministrationTool.Helper;
using AdministrationTool.Rest;
using ClosedXML.Excel;
using ClosedXML.Report;
using iSIM.Core.Common.Enum;
using iSIM.Core.Common.Model;
using iSIM.Core.Common.Security;
using iSIM.Core.Entity.Dto;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;


namespace AdministrationTool
{
    public partial class Form1 : Form
    {
        bool LoginIsSuccess = true;
        

        private List<UnifiedAssetDto> _assetList;

        private RadTreeView RtvCameras { get; set; }

        SessionUser user = null;

        UnifiedAssetTreeItem parentTreeItem;

        private static readonly object _rtvCamerasLockObject = new object();
        DataTable dataTable = null;

        Microsoft.Office.Interop.Excel.Application excel;
        Microsoft.Office.Interop.Excel.Workbook worKbooK;
        Microsoft.Office.Interop.Excel.Worksheet worKsheeT;
        Microsoft.Office.Interop.Excel.Range celLrangE;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Form1()
        {
            try
            {
                Log.Info("InitializeComponent start");
                InitializeComponent();
                Log.Info("RtvCameras start");
                RtvCameras = new RadTreeView
                {
                    Dock = DockStyle.Fill,
                    Name = "RtvCameras",
                    SpacingBetweenNodes = -1,
                    TabIndex = 0,
                    ShowLines = true,
                    Location = new Point(0, 30),
                    //DataSource = _treeItemList,
                    DisplayMember = "Text",
                    ParentMember = "ParentId",
                    ChildMember = "Id",
                    ValueMember = "OriginalObject",
                };
                Log.Info("RtvCameras finish");
                pnlAsset.AutoScroll = true;
                var temp = new RadScrollablePanel { Dock = DockStyle.Fill };
                pnlAsset.Controls.Add(temp);
                RtvCameras.AutoScroll = true;
                temp.PanelContainer.Controls.Add(RtvCameras);
            }
            catch (Exception ex)
            {
                Log.Error("Exception ", ex);
            }

            
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {

                if (string.IsNullOrEmpty(txtUserName.Text))
                {
                    MessageBox.Show("usernameRequired", "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoginIsSuccess = false;
                }
                else if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("passwordRequired", "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoginIsSuccess = false;
                }
                else
                {

                    string isimRestUrl = txtRestAddress.Text;
                    bool connectionSuccess = IsimRestClient.Instance.SetConnectionInfo(isimRestUrl);
                    if (!connectionSuccess)
                    {
                        MessageBox.Show("CheckRestUrl", "Login Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoginIsSuccess = false;
                    }
                    else
                    {
                        btnLogin.Enabled = false;
                        string machineID = IsimCrypto.MakeMd5(CommonHelper.GetMachineId());
                        var resultClient = await IsimRestClient.Instance.GetClientByKeyAsync(machineID);
                        if (resultClient == null)
                        {
                            Log.Info("Client is not existing!!!");
                            LoginIsSuccess = false;
                            txtPassword.ResetText();
                            MessageBox.Show("Client is not existing", "Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnLogin.Enabled = true;
                            return;
                        }
                        AuthorizationHelper.Client = resultClient;
                        IsimResult<SessionUser> userResult = null;
                        

                        userResult = await IsimRestClient.Instance.Login(txtUserName.Text, txtPassword.Text);
                        user = userResult.Data;

                        if (userResult.StatusCode == ResultStatusCode.LoginFailed && userResult.Message.Contains("in-memory database"))
                        {
                            LoginIsSuccess = false;
                            txtPassword.ResetText();
                            MessageBox.Show("LoginFailed", "login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //OnWrongCredentials?.Invoke(this);
                        }
                        else if (!userResult.IsSuccess || user.UserId < 1)
                        {
                            LoginIsSuccess = false;

                            txtPassword.ResetText();
                            MessageBox.Show("usernamePasswordIncorrect", "login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //OnWrongCredentials?.Invoke(this);

                        }
                        else if (!string.IsNullOrEmpty(user.Username) && user.UserId < 1)
                        {
                            LoginIsSuccess = false;
                            txtPassword.ResetText();
                            MessageBox.Show("usernamePasswordIncorrect", "login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            //OnWrongCredentials?.Invoke(this);
                        }

                        //if (user != null && !user.Roles.Contains(Role.LoginAccess))
                        //{
                        //    // Log.Info(LanguageSupport.GetString("currentUserPermission"));
                        //    LoginIsSuccess = false;
                        //    txtPassword.ResetText();
                        //    MessageBox.Show("currentUserPermission", "login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //    // OnWrongCredentials?.Invoke(this);
                        //}
                        //IsimServer.Instance.LoginService().Login(txtUserName.Text, txtPassword.Text);

                    }

                    if (!LoginIsSuccess)
                    {
                        btnLogin.Enabled = true;
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Login Success, Asset List Will be Loaded", "login", MessageBoxButtons.OK, MessageBoxIcon.None);

                        btnExport.Visible = true;
                        LoadAssetList(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception ",ex);
            }

        }

        private async void LoadAssetList(bool updateCache = false)
        {
            try
            {

                lblStatus.Text = "Loading Asset Lists...";

                //_loadingPanel.Visible = true;

                //await Task.Factory.StartNew(async () =>
                //{
                //if (updateCache)
                //{
                //    Thread.Sleep(1000);
                //    AssetCache.Instance.RefreshCameras();
                //}
                // Load assets
                //todo burada binding değişince sürekli db ye gitme engellenecek
                _assetList = await IsimRestClient.Instance.GetAssetListByUserIdAsync(user.UserId);
                if (_assetList == null || _assetList.Count == 0)
                {
                    Log.Error("camera list : assetlist is null");
                    return;
                }

                lock (_rtvCamerasLockObject)
                {
                    // Load assets 
                    RtvCameras.InvokeIfRequired(() =>
                    {
                        RtvCameras.BeginEdit();
                        GenerateAssetList();
                        // Calculate counts
                        foreach (var item in RtvCameras.Nodes)
                        {
                            DecideChildrenCount(item);
                        }
                        RtvCameras.EndEdit();
                    });
                }
                lblStatus.Text = "Asset List Loaded Converting to DataTable...";
                dataTable = convertTreeViewToDataSet();
                lblStatus.Text = "Finished...";
                // convertToExcel(dataTable);
                //});
            }
            catch (Exception ex)
            {
                Log.Error("camera list :" + ex.Message.ToString());
            }
            //_loadingPanel.Visible = false;
        }

        private void GenerateAssetList()
        {
            RtvCameras.Nodes.Clear();

            if (_assetList == null) return;



            var assetlistOrdered = _assetList.OrderBy(x => x.Name).ToList();
            //var parentAssetDto = assetlistOrdered.FirstOrDefault();

            //parentTreeItem = new UnifiedAssetTreeItem { AssetDto = parentAssetDto, Text = parentAssetDto.Name, Name = parentAssetDto.Name, Tag = parentAssetDto.Id };
            parentTreeItem = new UnifiedAssetTreeItem {   Text = "Assets", Name = "Assets", Tag = 0 };

            RtvCameras.Nodes.Add(parentTreeItem);

            foreach (var assetDto in assetlistOrdered)
            {
                if (!assetDto.IsActive)
                    continue;

                if (RtvCameras.Nodes.Any(x => ((UnifiedAssetTreeItem)(x)).AssetDto?.Id == assetDto.Id))
                    continue;

                if (assetDto.AssetType == AssetType.AssetGroup && (assetDto.ChildAssetDtos == null || assetDto.ChildAssetDtos.Count == 0))
                    continue;

                if (assetDto.AssetType == AssetType.AssetGroup && !AnyChildAssetExist(assetDto))
                    continue;

                // Filter by asset type
                //if (assetDto.AssetType!=AssetType.Camera)
                //    continue;

                // Generate asset tree item - ROOT
                //Image img = null;
                //UnifiedAssetTreeItem UATreeItem = new UnifiedAssetTreeItem { AssetDto = assetDto, Text = assetDto.Name, Name = assetDto.Name, Tag = assetDto.Id };
                //parentTreeItem = UATreeItem;
                //if (assetImages.TryGetValue(assetDto.AssetType, out img))
                //{
                //    UATreeItem.Image = img;
                //}
                // Add

               

                // Recursively add child nodes
                if (assetDto.AssetType == AssetType.AssetGroup)
                {
                    AddChildAsset(assetDto, parentTreeItem);
                }
            }
        }

        private bool AnyChildAssetExist(UnifiedAssetDto assetDto)
        {
            if (assetDto.ChildAssetDtos == null || assetDto.ChildAssetDtos.Count == 0)
            {
                return false;
            }

            foreach (var childDto in assetDto.ChildAssetDtos)
            {
                // Skip passive assets
                if (!childDto.IsActive)
                {
                    continue;
                }

                // Filter by asset type
                if (childDto.AssetType != AssetType.AssetGroup)
                {
                    if (childDto.AssetType == AssetType.Camera)
                    {
                        return true;
                    }
                }
                else
                {
                    // If has asset group then check child assets if matches filter
                    bool result = AnyChildAssetExist(childDto);
                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddChildAsset(UnifiedAssetDto assetDto, UnifiedAssetTreeItem treeItem)
        {
            foreach (var childAssetDto in assetDto.ChildAssetDtos)
            {
                if (childAssetDto.AssetType == AssetType.AssetGroup && (childAssetDto.ChildAssetDtos == null || childAssetDto.ChildAssetDtos.Count == 0))
                    continue;

                if (childAssetDto.AssetType == AssetType.AssetGroup && !AnyChildAssetExist(childAssetDto))
                    continue;

                //// Filter by asset type
                //if (childAssetDto.AssetType!=AssetType.Camera)
                //    continue;

                if (!childAssetDto.IsActive)
                {
                    continue;
                }

                Image img = null;
                UnifiedAssetTreeItem UATreeItem = new UnifiedAssetTreeItem { AssetDto = childAssetDto, Text = childAssetDto.Name, Name = childAssetDto.Name, Tag = childAssetDto.Id };
                //if (assetImages.TryGetValue(childAssetDto.AssetType, out img))
                //{
                //    UATreeItem.Image = img;
                //}
                // Add
                RadTreeNode foundNode = FindNodeByValue(UATreeItem.Tag, RtvCameras.Nodes);
                if (foundNode != null)
                    Debug.WriteLine(foundNode.Tag);
                else
                {
                    if (childAssetDto.AssetType == AssetType.Camera)
                        parentTreeItem.Nodes.Add(UATreeItem);
                    Debug.WriteLine(UATreeItem.Text);
                }

                // Recursively add child nodes
                if (childAssetDto.AssetType == AssetType.AssetGroup)
                {
                    AddChildAsset(childAssetDto, UATreeItem);


                }
            }
        }

        private static void DecideChildrenCount(RadTreeNode node)
        {
            try
            {
                if (node.Nodes.Count == 0)
                {
                    if (!node.Visible)
                        return;

                    var tmpNode = node.Parent;

                    while (tmpNode != null)
                    {
                        var item = tmpNode.DataBoundItem as TreeItem;
                        if (item != null)
                        {
                            item.ValuableChildrenCount += 1;
                            item.Text = string.Format("({0})", item.ValuableChildrenCount);
                            item.Text = item.Text + string.Format("{0}", item.Name);
                            //item.Text = string.Format("{0} )({1}\u200e", item.Name, item.ValuableChildrenCount);
                        }

                        //if (LedaCommon.Helper.ApplicationHelper.CurrentCulture != null &&
                        //    LedaCommon.Helper.ApplicationHelper.CurrentCulture.Name == "ar-QA")
                        //{
                        //    tmpNode.Text = string.Format("{0} {1}", "(" + tmpNode.Name, "(" + tmpNode.Nodes.Count);
                        //}
                        //else
                        tmpNode.Text = string.Format("{0} ({1})", tmpNode.Name, tmpNode.Nodes.Count);

                        tmpNode = tmpNode.Parent;
                    }
                }
                else
                {
                    foreach (var n in node.Nodes)
                    {
                        DecideChildrenCount(n);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("DecideChildrenCount -> error :" + ex.Message);
            }


        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //exportFileDialog.ShowDialog();

            //if (exportFileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    string fileName;
            //    fileName = exportFileDialog.FileName;
            //    MessageBox.Show(fileName);
            //}
            btnExport.Enabled = false;
            string exportText = btnExport.Text;
            btnExport.Text = "Exporting ...";
            var result = Report(dataTable);
            btnExport.Text = exportText;
            btnExport.Enabled = true;
            if(result)
                MessageBox.Show("AssetList.xlsx File Exported Successfully", "Export", MessageBoxButtons.OK, MessageBoxIcon.None);
            else
                MessageBox.Show("An Error Occured", "Export", MessageBoxButtons.OK, MessageBoxIcon.None);

        }

        private RadTreeNode FindNodeByValue(object tagValue, Telerik.WinControls.UI.RadTreeNodeCollection nodes)
        {
            foreach (RadTreeNode node in nodes)
            {
                if (node.Tag.Equals(tagValue))
                {
                    return node;
                }
                else
                {
                    RadTreeNode n = FindNodeByValue(tagValue, node.Nodes);
                    if (n != null)
                    {
                        return n;
                    }
                }
            }

            return null;
        }

        private void AddNodeAtRoot(object tagValue, Telerik.WinControls.UI.RadTreeNodeCollection nodes)
        {
            foreach (RadTreeNode node in nodes)
            {
                if (node.Tag.Equals(tagValue))
                {
                    // return node;
                }
                else
                {
                    RadTreeNode n = FindNodeByValue(tagValue, node.Nodes);
                    if (n != null)
                    {
                        // return n;
                    }
                }
            }

            // return null;
        }

        public void CreateSaveFile(string fileName, RadTreeView treeView)
        {
            using (StreamWriter streamWriter = new StreamWriter(fileName))
            {
                // Print each node recursively.
                RadTreeNodeCollection nodes = treeView.Nodes;

                foreach (RadTreeNode n in nodes)
                {
                    WriteRecursive(streamWriter, n);
                }
                streamWriter.Close(); // Optional since we have "using", but good practice to include.
            }
        }

        public void WriteRecursive(StreamWriter writer, RadTreeNode treeNode)
        {
            var node = (UnifiedAssetTreeItem)treeNode;
            if (node.AssetDto.AssetType == AssetType.AssetGroup)
            {
                writer.WriteLine(String.Format("{0} {1}", treeNode.Text, treeNode.Nodes.Count));
                // Print each node recursively.
                foreach (RadTreeNode tn in treeNode.Nodes)
                {
                    WriteRecursive(writer, tn);
                }
            }
        }

        private DataTable convertTreeViewToDataSet()
        {
            DataSet customerDS = new DataSet();
            //DataTable custTable = customerDS.Tables.Add("CustTable");
            DataTable dt = new DataTable("Values");
            dt.Columns.Add("AssetName", typeof(string));
            for (int i = 0; i < parentTreeItem.Nodes.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dr["AssetName"] = parentTreeItem.Nodes[i].Text;
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private bool Report(DataTable dataTable)
        {
            bool success;
            try
            {
                var wb = new XLWorkbook();

                var dTable = dataTable;

                // Add a DataTable as a worksheet
                wb.Worksheets.Add(dTable);

                wb.SaveAs("AssetList.xlsx");
                success = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Report Exception {0}", ex);
                success = false;
            }
            return success;
        }

    }
}
