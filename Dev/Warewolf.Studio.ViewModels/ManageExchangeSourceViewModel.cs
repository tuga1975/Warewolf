﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Practices.Prism.PubSubEvents;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageExchangeSourceViewModel : SourceBaseImpl<IExchangeSource>, IManageExchangeSourceViewModel
    {
        string _autoDiscoverUrl;
        string _userName;
        string _password;
        int _timeout;
        string _testMessage;
        string _emailTo;
        string _resourceName;

        IExchangeSource _emailServiceSource;
        readonly IManageExchangeSourceModel _updateManager;
        CancellationTokenSource _token;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _headerText;
        bool _enableSend;

        bool _isDisposed;

        public ManageExchangeSourceViewModel(IManageExchangeSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator) : this(updateManager, aggregator)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _updateManager = updateManager;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            HeaderText = Resources.Languages.Core.ExchangeSourceNewHeaderLabel;
            Header = Resources.Languages.Core.ExchangeSourceNewHeaderLabel;
            AutoDiscoverUrl = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            Timeout = 10000;
        }

        public ManageExchangeSourceViewModel(IManageExchangeSourceModel updateManager, IEventAggregator aggregator, IExchangeSource exchangeSource, IAsyncWorker AsyncWorker)
            : this(updateManager, aggregator)
        {
            VerifyArgument.IsNotNull("exchangeSource", exchangeSource);
            AsyncWorker.Start(() => updateManager.FetchSource(exchangeSource.ResourceID), source =>
            {
                _emailServiceSource = source;
                _emailServiceSource.Path = exchangeSource.Path;
                FromModel(_emailServiceSource);
                SetupHeaderTextFromExisting();
            });

        }

        public ManageExchangeSourceViewModel(IManageExchangeSourceModel updateManager, IEventAggregator aggregator)
            : base("ExchangeSource")
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            SendCommand = new DelegateCommand(p=>TestConnection(), p => CanTest());
            OkCommand = new DelegateCommand(p => SaveConnection(), p => CanSave());
            Testing = false;
            _testPassed = false;
            _testFailed = false;
        }

        public ManageExchangeSourceViewModel()
            : base("ExchangeSource")
        {

        }

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = value;
            }
        }

        public override void FromModel(IExchangeSource source)
        {
            AutoDiscoverUrl = source.AutoDiscoverUrl;
            UserName = source.UserName;
            Password = source.Password;
            Timeout = source.Timeout;
        }

        void SetupHeaderTextFromExisting()
        {
            if (_emailServiceSource != null)
            {
                HeaderText = (_emailServiceSource.ResourceName ?? ResourceName).Trim();
                Header = (_emailServiceSource.ResourceName ?? ResourceName).Trim();
            }
        }

        public override bool CanSave() => !string.IsNullOrWhiteSpace(AutoDiscoverUrl);

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (string.IsNullOrEmpty(AutoDiscoverUrl) && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
            {
                return false;
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }

        public override void Save()
        {
            SaveConnection();
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    SetupHeaderTextFromExisting();
                }
                OnPropertyChanged(_resourceName);
            }
        }

        void SaveConnection()
        {
            if (_emailServiceSource == null)
            {
                var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                var res = requestServiceNameViewModel.ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    var src = ToSource();
                    src.ResourceName = requestServiceNameViewModel.ResourceName.Name;
                    src.Path = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.ResourceID);
                    }

                    Item = src;
                    _emailServiceSource = src;
                    ResourceName = _emailServiceSource.ResourceName;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _emailServiceSource = src;
                SetupHeaderTextFromExisting();
            }
            TestPassed = false;
        }

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

        void Save(IExchangeSource source)
        {
            _updateManager.Save(source);
        }

        public string AutoDiscoverUrl
        {
            get { return _autoDiscoverUrl; }
            set
            {
                if (value != _autoDiscoverUrl)
                {
                    _autoDiscoverUrl = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => AutoDiscoverUrl);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => UserName);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value != _password)
                {
                    _password = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => Password);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value != _timeout)
                {
                    _timeout = value;
                    TestMessage = string.Empty;

                    if (!_timeout.ToString().IsNumeric())
                    {
                        OkCommand.CanExecute(false);
                    }

                    OnPropertyChanged(() => Timeout);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string EmailTo
        {
            get { return _emailTo; }
            set
            {
                if (value != _emailTo)
                {
                    _emailTo = value;
                    TestMessage = string.Empty;

                    EnableSend = true;
                    if (!_emailTo.IsEmail())
                    {
                        EnableSend = false;
                    }

                    OnPropertyChanged(() => EmailTo);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => EnableSend);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        public void TestConnection()
        {
            _token = new CancellationTokenSource();


            var t = new Task(SetupProgressSpinner, _token.Token);

            t.ContinueWith(a => Application.Current?.Dispatcher.Invoke(() =>
            {
                if (!_token.IsCancellationRequested)
                {
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                            {
                                TestFailed = true;
                                TestPassed = false;
                                Testing = false;
                                TestMessage = GetExceptionMessage(t.Exception);
                                break;
                            }
                        case TaskStatus.RanToCompletion:
                            {
                                TestMessage = "Passed";
                                TestFailed = false;
                                TestPassed = true;
                                Testing = false;
                                break;
                            }

                        case TaskStatus.Created:
                            break;
                        case TaskStatus.WaitingForActivation:
                            break;
                        case TaskStatus.WaitingToRun:
                            break;
                        case TaskStatus.Running:
                            break;
                        case TaskStatus.WaitingForChildrenToComplete:
                            break;
                        case TaskStatus.Canceled:
                            break;
                        default:
                            break;
                    }
                }
            }));
            t.Start();
        }

        void SetupProgressSpinner()
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            _updateManager.TestConnection(ToNewSource());
        }

        IExchangeSource ToNewSource()
        {
            var resourceID = _emailServiceSource == null ? Guid.NewGuid() : _emailServiceSource.ResourceID;
            return new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = AutoDiscoverUrl,
                Password = Password,
                UserName = UserName,
                Timeout = Timeout,
                EmailTo = EmailTo,
                ResourceName = Name,
                ResourceType = "ExchangeSource",
                Id = resourceID,
                ResourceID = resourceID,
            };
        }

        public IExchangeSource ToSource()
        {
            if (_emailServiceSource == null)
            {
                var resourceID = Guid.NewGuid();
                return new ExchangeSourceDefinition
                {
                    AutoDiscoverUrl = AutoDiscoverUrl,
                    Password = Password,
                    UserName = UserName,
                    Timeout = Timeout,
                    EmailTo = EmailTo,
                    ResourceName = ResourceName,
                    ResourceType = "ExchangeSource",
                    Id = resourceID,
                    ResourceID = resourceID,
                }
                    ;
            }
            _emailServiceSource.AutoDiscoverUrl = AutoDiscoverUrl;
            _emailServiceSource.UserName = UserName;
            _emailServiceSource.Password = Password;
            _emailServiceSource.Timeout = Timeout;
            return _emailServiceSource;
        }

        public override IExchangeSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            var resourceID = _emailServiceSource == null ? Guid.NewGuid() : _emailServiceSource.ResourceID;
            return new ExchangeSourceDefinition
            {
                AutoDiscoverUrl = AutoDiscoverUrl,
                Password = Password,
                UserName = UserName,
                Timeout = Timeout,
                EmailTo = EmailTo,
                ResourceType = "ExchangeSource",
                ResourceName = ResourceName,
                Id = resourceID,
                ResourceID = resourceID,
            };
        }

        public bool TestFailed
        {
            get
            {
                return _testFailed;
            }
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }
        public bool Testing
        {
            get
            {
                return _testing;
            }
            set
            {
                _testing = value;
                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
            }
        }

        public string TestMessage
        {
            get { return _testMessage; }
            set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        public ICommand SendCommand { get; set; }
        public ICommand OkCommand { get; set; }

        public bool EnableSend
        {
            get { return _enableSend; }
            set
            {
                _enableSend = value;
                OnPropertyChanged(() => EnableSend);
            }
        }

        public string AutoDiscoverLabel => Resources.Languages.Core.AutoDiscoverLabel;

        public string UserNameLabel => Resources.Languages.Core.UserNameLabel;

        public string PasswordLabel => Resources.Languages.Core.PasswordLabel;

        public string TimeoutLabel => Resources.Languages.Core.EmailSourceTimeoutLabel;

        public string TestLabel => Resources.Languages.Core.TestConnectionLabel;

        public string EmailFromLabel => Resources.Languages.Core.EmailSourceEmailFromLabel;

        public string EmailToLabel => Resources.Languages.Core.EmailSourceEmailToLabel;

        protected override void OnDispose()
        {
            if (RequestServiceNameViewModel != null)
            {
                RequestServiceNameViewModel.Result?.Dispose();
                RequestServiceNameViewModel.Dispose();
            }
            DisposeManageExchangeSourceViewModel(true);
        }

        void DisposeManageExchangeSourceViewModel(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _token?.Dispose();
                }
                _isDisposed = true;
            }
        }
    }
}
