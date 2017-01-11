﻿
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class Folders
    {
        const string ResourceCreatedInFolder = "Resource Created In Folder";
        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFolders_InFileredExplorer_UITest()
        {
            UIMap.Filter_Explorer("DragAndDropMergeFolder");
            UIMap.Drag_Explorer_First_Item_Onto_Second_Item();
            UIMap.Filter_Explorer("DragAndDropMergeResource1");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.Exists, "Resource did not merge into folder after drag and drop in the explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void MergeFolders_InUnFileredExplorer_UITest()
        {
            UIMap.TryClearExplorerFilter();
            UIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FirstItem.Exists, "Resource did not merge into folder after drag and drop in an unfiltered explorer UI.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Create_Resource_InFolderUITest()
        {            
            UIMap.Filter_Explorer("Acceptance Tests");
            UIMap.Create_New_Workflow_In_Explorer_First_Item_With_Context_Menu();
            UIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog(ResourceCreatedInFolder);
            UIMap.Filter_Explorer("Resource Created In Folder");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateNewFolderInLocalHostUsingShortcutKeysUITest()
        {
            UIMap.Click_LocalHost_Once();
            UIMap.Create_New_Folder_Using_Shortcut();
            UIMap.Filter_Explorer("New Folder");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
        }
        
        [TestMethod]
        [TestCategory("Explorer")]
        public void Right_Click_On_The_FolderCount_ContextMenu_UITest()
        {
            UIMap.Right_Click_On_The_Folder_Count();
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.ErrorWindow), "The studio throws an error when you right click on the folder count part of the explorer.");
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}