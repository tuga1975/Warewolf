﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by coded UI test builder.
//      Version: 15.0.0.0
//
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------

namespace Warewolf.UI.Tests.Merge.MergeConflictsUIMapClasses
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Input;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public partial class MergeConflictsUIMap
    {
        
        #region Properties
        public UIWarewolfDEV2PIETERTEWindow UIWarewolfDEV2PIETERTEWindow
        {
            get
            {
                if ((this.mUIWarewolfDEV2PIETERTEWindow == null))
                {
                    this.mUIWarewolfDEV2PIETERTEWindow = new UIWarewolfDEV2PIETERTEWindow();
                }
                return this.mUIWarewolfDEV2PIETERTEWindow;
            }
        }
        #endregion
        
        #region Fields
        private UIWarewolfDEV2PIETERTEWindow mUIWarewolfDEV2PIETERTEWindow;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIWarewolfDEV2PIETERTEWindow : WpfWindow
    {
        
        public UIWarewolfDEV2PIETERTEWindow()
        {
            #region Search Criteria
            this.SearchProperties[WpfWindow.PropertyNames.Name] = "Warewolf (DEV2\\PIETER.TERBLANCHE)";
            this.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public UIDev2StudioViewModelsCustom UIDev2StudioViewModelsCustom
        {
            get
            {
                if ((this.mUIDev2StudioViewModelsCustom == null))
                {
                    this.mUIDev2StudioViewModelsCustom = new UIDev2StudioViewModelsCustom(this);
                }
                return this.mUIDev2StudioViewModelsCustom;
            }
        }
        
        public UIItemCustom UIItemCustom
        {
            get
            {
                if ((this.mUIItemCustom == null))
                {
                    this.mUIItemCustom = new UIItemCustom(this);
                }
                return this.mUIItemCustom;
            }
        }
        
        public UIUI_WorkflowDesigner_Custom UIUI_WorkflowDesigner_Custom
        {
            get
            {
                if ((this.mUIUI_WorkflowDesigner_Custom == null))
                {
                    this.mUIUI_WorkflowDesigner_Custom = new UIUI_WorkflowDesigner_Custom(this);
                }
                return this.mUIUI_WorkflowDesigner_Custom;
            }
        }
        #endregion
        
        #region Fields
        private UIDev2StudioViewModelsCustom mUIDev2StudioViewModelsCustom;
        
        private UIItemCustom mUIItemCustom;
        
        private UIUI_WorkflowDesigner_Custom mUIUI_WorkflowDesigner_Custom;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIDev2StudioViewModelsCustom : WpfCustom
    {
        
        public UIDev2StudioViewModelsCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.ContentPane";
            this.SearchProperties[WpfControl.PropertyNames.AutomationId] = "Dev2.Studio.ViewModels.WorkSurface.WorkSurfaceContextViewModel";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public UIContentDockManagerCustom UIContentDockManagerCustom
        {
            get
            {
                if ((this.mUIContentDockManagerCustom == null))
                {
                    this.mUIContentDockManagerCustom = new UIContentDockManagerCustom(this);
                }
                return this.mUIContentDockManagerCustom;
            }
        }
        #endregion
        
        #region Fields
        private UIContentDockManagerCustom mUIContentDockManagerCustom;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIContentDockManagerCustom : WpfCustom
    {
        
        public UIContentDockManagerCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.XamDockManager";
            this.SearchProperties[WpfControl.PropertyNames.AutomationId] = "ContentDockManager";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public WpfCustom UIItemCustom
        {
            get
            {
                if ((this.mUIItemCustom == null))
                {
                    this.mUIItemCustom = new WpfCustom(this);
                    #region Search Criteria
                    this.mUIItemCustom.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.MergeWorkflowView";
                    this.mUIItemCustom.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
                    #endregion
                }
                return this.mUIItemCustom;
            }
        }
        
        public WpfPane UIScrollViewerPane
        {
            get
            {
                if ((this.mUIScrollViewerPane == null))
                {
                    this.mUIScrollViewerPane = new WpfPane(this);
                    #region Search Criteria
                    this.mUIScrollViewerPane.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.ScrollViewer";
                    this.mUIScrollViewerPane.SearchProperties[WpfPane.PropertyNames.AutomationId] = "ScrollViewer";
                    this.mUIScrollViewerPane.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
                    #endregion
                }
                return this.mUIScrollViewerPane;
            }
        }
        #endregion
        
        #region Fields
        private WpfCustom mUIItemCustom;
        
        private WpfPane mUIScrollViewerPane;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIItemCustom : WpfCustom
    {
        
        public UIItemCustom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.MergeWorkflowView";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public UIScrollViewerPane UIScrollViewerPane
        {
            get
            {
                if ((this.mUIScrollViewerPane == null))
                {
                    this.mUIScrollViewerPane = new UIScrollViewerPane(this);
                }
                return this.mUIScrollViewerPane;
            }
        }
        
        public UIUI_MergeVariablesExpExpander UIUI_MergeVariablesExpExpander
        {
            get
            {
                if ((this.mUIUI_MergeVariablesExpExpander == null))
                {
                    this.mUIUI_MergeVariablesExpExpander = new UIUI_MergeVariablesExpExpander(this);
                }
                return this.mUIUI_MergeVariablesExpExpander;
            }
        }
        #endregion
        
        #region Fields
        private UIScrollViewerPane mUIScrollViewerPane;
        
        private UIUI_MergeVariablesExpExpander mUIUI_MergeVariablesExpExpander;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIScrollViewerPane : WpfPane
    {
        
        public UIScrollViewerPane(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.ScrollViewer";
            this.SearchProperties[WpfPane.PropertyNames.AutomationId] = "ScrollViewer";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public WpfTree UIUI_MergeTreeView_AutTree
        {
            get
            {
                if ((this.mUIUI_MergeTreeView_AutTree == null))
                {
                    this.mUIUI_MergeTreeView_AutTree = new WpfTree(this);
                    #region Search Criteria
                    this.mUIUI_MergeTreeView_AutTree.SearchProperties[WpfTree.PropertyNames.AutomationId] = "UI_MergeTreeView_AutoID";
                    this.mUIUI_MergeTreeView_AutTree.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
                    #endregion
                }
                return this.mUIUI_MergeTreeView_AutTree;
            }
        }
        #endregion
        
        #region Fields
        private WpfTree mUIUI_MergeTreeView_AutTree;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIUI_MergeVariablesExpExpander : WpfExpander
    {
        
        public UIUI_MergeVariablesExpExpander(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfExpander.PropertyNames.AutomationId] = "UI_MergeVariablesExpander_AutoID";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public WpfButton UIHeaderSiteButton
        {
            get
            {
                if ((this.mUIHeaderSiteButton == null))
                {
                    this.mUIHeaderSiteButton = new WpfButton(this);
                    #region Search Criteria
                    this.mUIHeaderSiteButton.SearchProperties[WpfButton.PropertyNames.AutomationId] = "HeaderSite";
                    this.mUIHeaderSiteButton.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
                    #endregion
                }
                return this.mUIHeaderSiteButton;
            }
        }
        #endregion
        
        #region Fields
        private WpfButton mUIHeaderSiteButton;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIUI_WorkflowDesigner_Custom : WpfCustom
    {
        
        public UIUI_WorkflowDesigner_Custom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.WorkflowDesignerView";
            this.SearchProperties[WpfControl.PropertyNames.AutomationId] = "UI_WorkflowDesigner_AutoID";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public UIUserControl_1Custom UIUserControl_1Custom
        {
            get
            {
                if ((this.mUIUserControl_1Custom == null))
                {
                    this.mUIUserControl_1Custom = new UIUserControl_1Custom(this);
                }
                return this.mUIUserControl_1Custom;
            }
        }
        #endregion
        
        #region Fields
        private UIUserControl_1Custom mUIUserControl_1Custom;
        #endregion
    }
    
    [GeneratedCode("Coded UITest Builder", "15.0.26208.0")]
    public class UIUserControl_1Custom : WpfCustom
    {
        
        public UIUserControl_1Custom(UITestControl searchLimitContainer) : 
                base(searchLimitContainer)
        {
            #region Search Criteria
            this.SearchProperties[WpfControl.PropertyNames.ClassName] = "Uia.DesignerView";
            this.SearchProperties[WpfControl.PropertyNames.AutomationId] = "UserControl_1";
            this.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
            #endregion
        }
        
        #region Properties
        public WpfPane UIScrollViewerPane
        {
            get
            {
                if ((this.mUIScrollViewerPane == null))
                {
                    this.mUIScrollViewerPane = new WpfPane(this);
                    #region Search Criteria
                    this.mUIScrollViewerPane.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.ScrollViewer";
                    this.mUIScrollViewerPane.SearchProperties[WpfPane.PropertyNames.AutomationId] = "scrollViewer";
                    this.mUIScrollViewerPane.WindowTitles.Add("Warewolf (DEV2\\PIETER.TERBLANCHE)");
                    #endregion
                }
                return this.mUIScrollViewerPane;
            }
        }
        #endregion
        
        #region Fields
        private WpfPane mUIScrollViewerPane;
        #endregion
    }
}