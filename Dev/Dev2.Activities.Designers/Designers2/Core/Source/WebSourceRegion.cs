﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;






namespace Dev2.Activities.Designers2.Core.Source
{
    public class WebSourceRegion : ISourceToolRegion<IWebServiceSource>
    {
        IWebServiceSource _selectedSource;
        ICollection<IWebServiceSource> _sources;
        readonly ModelItem _modelItem;
        readonly Dictionary<Guid, IList<IToolRegion>> _previousRegions = new Dictionary<Guid, IList<IToolRegion>>();
        Guid _sourceId;
        Action _sourceChangedAction;
        double _labelWidth;
        string _newSourceHelpText;
        string _editSourceHelpText;
        string _sourcesHelpText;
        string _newSourceToolText;
        string _editSourceToolText;
        string _sourcesToolText;

        public WebSourceRegion(IWebServiceModel model, ModelItem modelItem)
        {
            LabelWidth = 46;
            ToolRegionName = "SourceRegion";
            Dependants = new List<IToolRegion>();
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(model.CreateNewSource);
            EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => model.EditSource(SelectedSource), CanEditSource);
            var sources = model.RetrieveSources().OrderBy(source => source.Name);
            Sources = sources.ToObservableCollection();
            IsEnabled = true;
            _modelItem = modelItem;
            SourceId = modelItem.GetProperty<Guid>("SourceId");
            SourcesHelpText = Warewolf.Studio.Resources.Languages.HelpText.WebServiceSourcesHelp;
            EditSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.WebServiceSelectedSourceHelp;
            NewSourceHelpText = Warewolf.Studio.Resources.Languages.HelpText.WebServiceNewWebSourceHelp;

            SourcesTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWebServiceSourcesTooltip;
            EditSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWebServiceEditSourceTooltip;
            NewSourceTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ManageWebServiceNewSourceTooltip;

            if (SourceId != Guid.Empty)
            {
                SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
            }
        }

        public string NewSourceHelpText
        {
            get
            {
                return _newSourceHelpText;
            }
            set
            {
                _newSourceHelpText = value;
                OnPropertyChanged();
            }
        }

        public string EditSourceHelpText
        {
            get
            {
                return _editSourceHelpText;
            }
            set
            {
                _editSourceHelpText = value;
                OnPropertyChanged();
            }
        }
        public string SourcesHelpText
        {
            get
            {
                return _sourcesHelpText;
            }
            set
            {
                _sourcesHelpText = value;
                OnPropertyChanged();
            }
        }

        public string NewSourceTooltip
        {
            get
            {
                return _newSourceToolText;
            }
            set
            {
                _newSourceToolText = value;
                OnPropertyChanged();
            }
        }

        public string EditSourceTooltip
        {
            get
            {
                return _editSourceToolText;
            }
            set
            {
                _editSourceToolText = value;
                OnPropertyChanged();
            }
        }
        public string SourcesTooltip
        {
            get
            {
                return _sourcesToolText;
            }
            set
            {
                _sourcesToolText = value;
                OnPropertyChanged();
            }
        }

        public WebSourceRegion()
        {
        }

        Guid SourceId
        {
            get
            {
                return _sourceId;
            }
            set
            {
                _sourceId = value;
                _modelItem?.SetProperty("SourceId", value);
            }
        }

        public bool CanEditSource() => SelectedSource != null;

        public ICommand EditSourceCommand { get; set; }

        public ICommand NewSourceCommand { get; set; }
        public Action SourceChangedAction
        {
            get
            {
                return _sourceChangedAction ?? (() => { });
            }
            set
            {
                _sourceChangedAction = value;
            }
        }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth
        {
            get
            {
                return _labelWidth;
            }
            set
            {
                _labelWidth = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion() => new WebSourceRegion
        {
            SelectedSource = SelectedSource
        };

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is WebSourceRegion region)
            {
                SelectedSource = region.SelectedSource;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of ISourceToolRegion<IWebServiceSource>

        public IWebServiceSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(value, _selectedSource) && _selectedSource != null && !string.IsNullOrEmpty(_selectedSource.HostName))
                {
                    StorePreviousValues(_selectedSource.Id);
                }

                if (Dependants != null)
                {
                    var outputs = Dependants.FirstOrDefault(a => a is IOutputsToolRegion);
                    if (outputs is OutputsRegion region)
                    {
                        region.Outputs = new ObservableCollection<IServiceOutputMapping>();
                        region.RecordsetName = string.Empty;
                        region.ObjectResult = string.Empty;
                        region.IsObject = false;
                        region.ObjectName = string.Empty;
                        
                    }
                }
                RestoreIfPrevious(value);
                OnPropertyChanged();
            }
        }

        void RestoreIfPrevious(IWebServiceSource value)
        {
            if (IsAPreviousValue(value) && _selectedSource != null)
            {
                RestorePreviousValues(value);
                SetSelectedSource(value);
            }
            else
            {
                SetSelectedSource(value);
                SourceChangedAction?.Invoke();
                OnSomethingChanged(this);
            }
            var delegateCommand = EditSourceCommand as Microsoft.Practices.Prism.Commands.DelegateCommand;
            delegateCommand?.RaiseCanExecuteChanged();

            _selectedSource = value;
        }

        void SetSelectedSource(IWebServiceSource value)
        {
            if (value != null)
            {
                _selectedSource = value;
                SavedSource = value;
                SourceId = value.Id;
            }
            OnPropertyChanged("SelectedSource");
        }

        void StorePreviousValues(Guid id)
        {
            _previousRegions.Remove(id);
            _previousRegions[id] = new List<IToolRegion>(Dependants.Select(a => a.CloneRegion()));
        }

        void RestorePreviousValues(IWebServiceSource value)
        {
            var toRestore = _previousRegions[value.Id];
            foreach (var toolRegion in Dependants.Zip(toRestore, (a, b) => new Tuple<IToolRegion, IToolRegion>(a, b)))
            {
                toolRegion.Item1.RestoreRegion(toolRegion.Item2);
            }
        }

        bool IsAPreviousValue(IWebServiceSource value) => _previousRegions.Keys.Any(a => a == value.Id);

        public ICollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                OnPropertyChanged();
            }
        }

        public IList<string> Errors => SelectedSource == null ? new List<string> { "Invalid Source Selected" } : new List<string>();

        public IWebServiceSource SavedSource
        {
            get
            {
                return _modelItem.GetProperty<IWebServiceSource>("SavedSource");
            }
            set
            {
                _modelItem.SetProperty("SavedSource", value);
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSomethingChanged(IToolRegion args)
        {
            var handler = SomethingChanged;
            handler?.Invoke(this, args);
        }
    }
}
