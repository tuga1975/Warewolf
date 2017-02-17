﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.DebugOutputTests
{
    [CodedUITest]
    public class DebugOutputWindowTests
    {
        const string SelectionHighlightWf = "SelectionHighlightWf";
        [TestMethod]
        [TestCategory("Debug Input")]
        // ReSharper disable once InconsistentNaming
        public void WorkFlowSelection_Validation_UITest()
        {
            ExplorerUIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            ExplorerUIMap.Click_DesicionStep_InDebugOutput();
            var assignHasNoFocus = ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Filter_Explorer(SelectionHighlightWf);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        #endregion
    }
}